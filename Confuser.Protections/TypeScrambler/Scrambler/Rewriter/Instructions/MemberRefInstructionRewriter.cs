﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Confuser.Protections.TypeScramble.Scrambler.Rewriter.Instructions {
	class MemberRefInstructionRewriter : InstructionRewriter<MemberRef> {

		MethodInfo[] CreationFactoryMethods;
		public MemberRefInstructionRewriter(IConfuserContext context) {
			ModuleDefMD md = ModuleDefMD.Load(typeof(EmbeddedCode.ObjectCreationFactory).Module);
			var logger = context.Registry.GetRequiredService<ILoggerFactory>().CreateLogger(TypeScrambleProtection._Id);

			MethodInfo[] tMethods = typeof(EmbeddedCode.ObjectCreationFactory).GetMethods(BindingFlags.Static | BindingFlags.Public);
			CreationFactoryMethods = new MethodInfo[tMethods.Length];
			foreach (var m in tMethods) {
				CreationFactoryMethods[m.GetParameters().Length] = m;
				logger.LogDebug("{0}] {1}", m.GetParameters().Length, m.Name);
			}
		}


		public override void ProcessOperand(TypeService service, MethodDef method, IList<Instruction> body, ref int index, MemberRef operand) {

			ScannedMethod current = service.GetItem(method.MDToken) as ScannedMethod;
			if (operand.MethodSig.Params.Count > 0 || current == null || body[index].OpCode != OpCodes.Newobj) {
				return;
			}

			ModuleDef mod = method.Module;


			var gettype = typeof(Type).GetMethod("GetTypeFromHandle");
			var createInstance = typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) });
			var createInstanceArgs = typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type), typeof(object[]) });

			TypeSig sig = null;

			if (operand.Class is TypeRef) {
				sig = (operand.Class as TypeRef)?.ToTypeSig();
			}
			if (operand.Class is TypeSpec) {
				sig = (operand.Class as TypeSpec)?.ToTypeSig();
			}

			if (sig != null) {

				//ScannedItem t = service.GetItem(operand.MDToken);
				//if (t != null) {
				//    sig = t.CreateGenericTypeSig(service.GetItem(method.DeclaringType.MDToken));
				// }
				var paramCount = operand.MethodSig.Params.Count;

				var gen = current.GetGeneric(sig);
				body[index].OpCode = OpCodes.Ldtoken;


				TypeSpecUser newTypeSpec = null;
				if (gen != null) {
					newTypeSpec = new TypeSpecUser(new GenericMVar(gen.Number));
				}
				else {
					newTypeSpec = new TypeSpecUser(sig);
				}
				body[index].Operand = newTypeSpec;

				/*
				var genericCallSig =  new GenericInstMethodSig( new TypeSig[] { current.ConvertToGenericIfAvalible(sig) });
				foreach(var param in operand.MethodSig.Params.Select(x => current.ConvertToGenericIfAvalible(x))) {
					genericCallSig.GenericArguments.Add(param);
				}

				/ tgtMethod.GenericInstMethodSig = genericCallSig;
				var spec = new MethodSpecUser(tgtMethod, genericCallSig);

				body[index].OpCode = OpCodes.Call;
				body[index].Operand = tgtMethod;
				*/

				body.Insert(++index, Instruction.Create(OpCodes.Call, mod.Import(gettype)));
				body.Insert(++index, Instruction.Create(OpCodes.Call, mod.Import(createInstance)));

			}
		}
	}
}
