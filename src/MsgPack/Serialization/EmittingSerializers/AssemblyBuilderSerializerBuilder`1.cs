#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2013 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
using System.Reflection.Emit;
using MsgPack.Serialization.AbstractSerializers;

namespace MsgPack.Serialization.EmittingSerializers
{
	/// <summary>
	///		An implementation of <see cref="SerializerBuilder{TContext,TConstruct,TObject}"/> with <see cref="AssemblyBuilder"/>.
	/// </summary>
	/// <typeparam name="TObject">The type of the serializing object.</typeparam>
	internal class AssemblyBuilderSerializerBuilder<TObject> : ILEmittingSerializerBuilder<AssemblyBuilderEmittingContext, TObject>
	{
		/// <summary>
		///		Initializes a new instance of the <see cref="AssemblyBuilderSerializerBuilder{TObject}"/> class for instance creation.
		/// </summary>
		public AssemblyBuilderSerializerBuilder() { }

		protected override ILConstruct EmitGetSerializerExpression( AssemblyBuilderEmittingContext context, Type targetType )
		{
			var instructions = context.Emitter.RegisterSerializer( targetType );
			return
				ILConstruct.Instruction(
					"getserializer",
					typeof( MessagePackSerializer<> ).MakeGenericType( targetType ),
					false,
				// Both of this pointer for FieldBasedSerializerEmitter and context argument of methods for ContextBasedSerializerEmitter are 0.
					il => instructions( il, 0 )
				);
		}

		protected override AssemblyBuilderEmittingContext CreateCodeGenerationContextForSerializerCreation( SerializationContext context )
		{
			return
				new AssemblyBuilderEmittingContext(
					context,
					typeof( TObject ),
					SerializationMethodGeneratorManager.Get().CreateEmitter( typeof( TObject ), EmitterFlavor.FieldBased )
				);
		}

		protected override void BuildSerializerCodeCore( ISerializerCodeGenerationContext context )
		{
			var asAssemblyBuilderCodeGenerationContext = context as AssemblyBuilderCodeGenerationContext;
			if ( asAssemblyBuilderCodeGenerationContext == null )
			{
				throw new ArgumentException(
					"'context' was not created with CreateGenerationContextForCodeGeneration method.",
					"context"
				);
			}

			var emittingContext =
				asAssemblyBuilderCodeGenerationContext.CreateEmittingContext(
					typeof( TObject )
				);

			this.BuildSerializer( emittingContext );
			// Finish type creation, and discard returned ctor.
			emittingContext.Emitter.CreateConstructor<TObject>();
		}
	}
}