﻿#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010 FUJIWARA, Yusuke
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace MsgPack.Serialization.Metadata
{
	internal static class _Packer
	{
		public static readonly MethodInfo PackMapHeader = FromExpression.ToMethod( ( Packer packer, int count ) => packer.PackMapHeader( count ) );
		public static readonly MethodInfo PackString = FromExpression.ToMethod( ( Packer packer, string value ) => packer.PackString( value ) );
	}

	internal static class _Unpacker
	{
		public static readonly MethodInfo Read = FromExpression.ToMethod( ( Unpacker unpacker ) => unpacker.Read() );
		public static readonly MethodInfo ReadSubtree = FromExpression.ToMethod( ( Unpacker unpacker ) => unpacker.ReadSubtree() );
		public static readonly PropertyInfo Data = FromExpression.ToProperty( ( Unpacker unpacker ) => unpacker.Data );
		public static readonly PropertyInfo ItemsCount = FromExpression.ToProperty( ( Unpacker unpacker ) => unpacker.ItemsCount );
	}


	internal static class _KeyValuePair<TKey, TValue>
	{
		public static readonly PropertyInfo Key = FromExpression.ToProperty( ( KeyValuePair<TKey, TValue> entry ) => entry.Key );
		public static readonly PropertyInfo Value = FromExpression.ToProperty( ( KeyValuePair<TKey, TValue> entry ) => entry.Value );
		public static readonly ConstructorInfo Ctor = FromExpression.ToConstructor( ( TKey key, TValue value ) => new KeyValuePair<TKey, TValue>( key, value ) );
	}

	internal static class _Nullable<T>
		where T : struct
	{
		public static readonly PropertyInfo HasValue = FromExpression.ToProperty( ( Nullable<T> nullable ) => nullable.HasValue );
		public static readonly PropertyInfo Value = FromExpression.ToProperty( ( Nullable<T> nullable ) => nullable.Value );
	}

	internal static class _IDisposable
	{
		public static readonly MethodInfo Dispose = FromExpression.ToMethod( ( IDisposable disposable ) => disposable.Dispose() );
	}

	internal static class _MessagePackObject
	{
		public static readonly MethodInfo AsString = FromExpression.ToMethod( ( MessagePackObject mpo ) => mpo.AsString() );
	}

	internal static class _String
	{
		public static readonly MethodInfo op_Equality = FromExpression.ToOperator( ( String left, String right ) => left == right );
		public static readonly MethodInfo op_Inequality = FromExpression.ToOperator( ( String left, String right ) => left != right );
	}
}
namespace MsgPack.Serialization.DefaultSerializers
{
	internal sealed class System_Collections_Generic_KeyValuePair_2MessagePackSerializer<TKey, TValue> : MessagePackSerializer<KeyValuePair<TKey, TValue>>
	{
		private readonly MessagePackSerializer<KeyValuePair<TKey, TValue>> _underlying;

		public System_Collections_Generic_KeyValuePair_2MessagePackSerializer( SerializationContext context )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			var emitter = SerializationMethodGeneratorManager.Get().CreateEmitter( typeof( KeyValuePair<TKey, TValue> ) );
			CreatePacker( emitter );
			CreateUnpacker( emitter );
			this._underlying = emitter.CreateInstance<KeyValuePair<TKey, TValue>>( context );
		}

		private static void CreatePacker( SerializerEmitter emitter )
		{
			var il = emitter.GetPackToMethodILGenerator();
			try
			{
				Emittion.EmitPackMambers(
					emitter,
					il,
					1,
					typeof( KeyValuePair<TKey, TValue> ),
					2,
					new Tuple<MemberInfo, Type>( Metadata._KeyValuePair<TKey, TValue>.Key, typeof( TKey ) ),
					new Tuple<MemberInfo, Type>( Metadata._KeyValuePair<TKey, TValue>.Value, typeof( TValue ) )
				);
				il.EmitRet();
			}
			finally
			{
				il.FlushTrace();
			}
		}

		private static void CreateUnpacker( SerializerEmitter emitter )
		{
			var il = emitter.GetUnpackFromMethodILGenerator();
			try
			{
				var key = il.DeclareLocal( typeof( TKey ), "key" );
				var value = il.DeclareLocal( typeof( TValue ), "value" );
				var isKeyFound = il.DeclareLocal( typeof( bool ), "isKeyFound" );
				var isValueFound = il.DeclareLocal( typeof( bool ), "isValueFound" );
				// while
				Emittion.EmitUnpackMembers(
					emitter,
					il,
					1,
					null,
					new Tuple<MemberInfo, string, LocalBuilder, LocalBuilder>( Metadata._KeyValuePair<TKey, TValue>.Key, "Key", key, isKeyFound ),
					new Tuple<MemberInfo, string, LocalBuilder, LocalBuilder>( Metadata._KeyValuePair<TKey, TValue>.Value, "Value", value, isValueFound )
				);

				var result = il.DeclareLocal( typeof( KeyValuePair<TKey, TValue> ), "result" );
				il.EmitAnyLdloca( result );
				il.EmitAnyLdloc( key );
				il.EmitAnyLdloc( value );
				il.EmitCallConstructor( Metadata._KeyValuePair<TKey, TValue>.Ctor );
				il.EmitAnyLdloc( result );
				il.EmitRet();
			}
			finally
			{
				il.FlushTrace();
			}
		}

		protected sealed override void PackToCore( Packer packer, KeyValuePair<TKey, TValue> objectTree )
		{
			this._underlying.PackTo( packer, objectTree );
		}

		protected sealed override KeyValuePair<TKey, TValue> UnpackFromCore( Unpacker unpacker )
		{
			return this._underlying.UnpackFrom( unpacker );
		}
	}
}