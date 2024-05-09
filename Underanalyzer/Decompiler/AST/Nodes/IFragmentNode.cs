using System;
using Underanalyzer.Decompiler.ControlFlow;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base type for all nodes which are valid fragments in the AST.
/// </summary>
public interface IFragmentNode : IASTNode
{
    /// <summary>
    /// Creates and builds new AST fragment node from the given control flow fragment.
    /// Determines which type of fragment node it is, and returns that type.
    /// </summary>
    internal static IFragmentNode Create(ASTBuilder builder, Fragment fragment)
    {
        // If we're at the root level, just use a block!
        if (fragment.StartAddress == 0)
        {
            BlockNode block = builder.BuildBlock(fragment.Children[0]);
            block.UseBraces = false;
            return block;
        }

        // Ensure we have a block after this fragment, so we can determine what it is
        if (fragment.Successors.Count != 1 || fragment.Successors[0] is not Block followingBlock)
        {
            throw new Exception("Expected block after fragment");
        }

        // Ensure we have enough instructions to work with
        if (followingBlock.Instructions.Count < 3)
        {
            throw new Exception("Missing instructions after fragment");
        }

        // Get function reference for fragment
        if (followingBlock.Instructions[0] is not { Kind: Opcode.Push, Type1: DataType.Int32, Function: IGMFunction function } || function is null)
        {
            throw new Exception("Expected push.i with function reference after fragment");
        }

        // Ensure conv instruction exists
        if (followingBlock.Instructions[1] is not { Kind: Opcode.Convert, Type1: DataType.Int32, Type2: DataType.Variable})
        {
            throw new Exception("Expected conv.i.v instruction after fragment");
        }

        switch (followingBlock.Instructions[2].Kind)
        {
            case Opcode.PushImmediate:
                {
                    // Normal function. Skip past basic instructions.
                    if (followingBlock.Instructions is not 
                        [   
                            _, _, 
                            { ValueShort: -1 or -16 }, 
                            { Kind: Opcode.Convert, Type1: DataType.Int32, Type2: DataType.Variable },
                            { Kind: Opcode.Call, Function.Name.Content: VMConstants.MethodFunction },
                            ..
                        ])
                    {
                        throw new Exception("Fragment instruction match failure (normal function)");
                    }

                    // Build body of the function
                    builder.PushFragmentContext();
                    BlockNode block = builder.BuildBlock(fragment.Children[0]);
                    builder.PopFragmentContext();

                    // Check if we have a name
                    if (followingBlock.Instructions is
                        [
                            _, _, _, _, _, 
                            { Kind: Opcode.Duplicate, DuplicationSize2: 0 },
                            { Kind: Opcode.PushImmediate },
                            { Kind: Opcode.Pop, Variable.Name.Content: string funcName },
                            ..
                        ])
                    {
                        // We have a name! Build and return result
                        builder.StartBlockInstructionIndex = 8;
                        return new FunctionDeclNode(funcName, false, block);
                    }

                    // We're anonymous!
                    builder.StartBlockInstructionIndex = 5;
                    return new FunctionDeclNode(null, false, block);
                }
            case Opcode.Call:
                {
                    // This is a struct or constructor function
                    if (followingBlock.Instructions is not
                        [
                            _, _,
                            { Kind: Opcode.Call, Function.Name.Content: VMConstants.NullObjectFunction },
                            { Kind: Opcode.Call, Function.Name.Content: VMConstants.MethodFunction },
                            ..
                        ])
                    {
                        throw new Exception("Fragment instruction match failure (struct/constructor)");
                    }

                    // Check if we're a struct or function constructor (named)
                    if (followingBlock.Instructions is
                        [
                            _, _, _, _,
                            { Kind: Opcode.Duplicate, DuplicationSize2: 0 },
                            { Kind: Opcode.PushImmediate, ValueShort: short pushVal },
                            { Kind: Opcode.Pop, Variable.Name.Content: string funcName },
                            ..
                        ])
                    {
                        // Check if struct or constructor
                        if (pushVal == -16)
                        {
                            // We're a struct
                            if (followingBlock.Instructions.Count < 8 ||
                                followingBlock.Instructions[7] is not 
                                { 
                                    Kind: Opcode.Call, 
                                    Function.Name.Content: VMConstants.NewObjectFunction,
                                    ArgumentCount: int argumentCount
                                })
                            {
                                throw new Exception("Fragment instruction match failure (struct)");
                            }

                            // Load struct arguments from stack (in reverse)
                            builder.PushFragmentContext();
                            builder.StructArguments = new(argumentCount - 1);
                            for (int i = 0; i < argumentCount - 1; i++)
                            {
                                builder.StructArguments.Add(builder.ExpressionStack.Pop());
                            }

                            // Build body
                            BlockNode block = builder.BuildBlock(fragment.Children[0]);
                            builder.PopFragmentContext();

                            builder.StartBlockInstructionIndex = 8;
                            return new StructNode(block);
                        }
                        else
                        {
                            // We're a constructor

                            // Build body
                            builder.PushFragmentContext();
                            BlockNode block = builder.BuildBlock(fragment.Children[0]);
                            builder.PopFragmentContext();

                            builder.StartBlockInstructionIndex = 7;
                            return new FunctionDeclNode(funcName, true, block);
                        }
                    }
                    else
                    {
                        // We're an anonymous constructor

                        // Build body
                        builder.PushFragmentContext();
                        BlockNode block = builder.BuildBlock(fragment.Children[0]);
                        builder.PopFragmentContext();

                        builder.StartBlockInstructionIndex = 4;
                        return new FunctionDeclNode(null, true, block);
                    }
                }
        }

        throw new Exception("Failed to detect type of fragment");
    }
}
