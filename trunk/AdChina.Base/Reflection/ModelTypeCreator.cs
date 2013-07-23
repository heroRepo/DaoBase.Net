using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace AdChina.Base.Reflection
{
    public class ClassFactory
    {
        public static readonly ClassFactory Instance = new ClassFactory();

        static ClassFactory() { }

        readonly ModuleBuilder _module;
        readonly Dictionary<Signature, Type> _classes;
        int _classCount;
        private AssemblyBuilder assembly;
        private ClassFactory()
        {
            var name = new AssemblyName("DynamicClasses");
            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            //assembly.DefineVersionInfoResource();
            _module = assembly.DefineDynamicModule("Module");
            _classes = new Dictionary<Signature, Type>();
        }

        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
        {
            var signature = new Signature(properties);
            lock (_classes)
            {
                Type type;
                if (!_classes.TryGetValue(signature, out type))
                {
                    type = CreateDynamicClass(signature.Properties);
                    _classes[signature] = type;
                } //assembly.Save("DynamicClasses.dll");
                return type;
            }

        }

        Type CreateDynamicClass(DynamicProperty[] properties)
        {
            var typeName = "DynamicClass" + (_classCount + 1);
            var tb = _module.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(DynamicClass));
            var fields = GenerateProperties(tb, properties);
            GenerateEquals(tb, fields);
            GenerateGetHashCode(tb, fields);
            GenerateConstructor(tb, fields);
            var result = tb.CreateType();
            _classCount++;
            return result;
        }

        static void GenerateConstructor(TypeBuilder tb, IList<FieldInfo> fieldInfos)
        {
            var builder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, fieldInfos.Select(p => p.FieldType).ToArray());
            for (var i = 0; i < fieldInfos.Count; i++)
            {
                builder.DefineParameter(i + 1, ParameterAttributes.None, "m" + fieldInfos[i].Name.Trim('_'));
            }

            var genGet = builder.GetILGenerator();
            genGet.Emit(OpCodes.Ldarg_0);
            genGet.Emit(OpCodes.Call, tb.BaseType.GetConstructor(System.Type.EmptyTypes));
            genGet.Emit(OpCodes.Nop);
            genGet.Emit(OpCodes.Nop);
            for (var i = 0; i < fieldInfos.Count; i++)
            {
                var op = OpCodes.Ldarg_S;
                switch (i)
                {
                    case 0:
                        op = OpCodes.Ldarg_1;
                        break;
                    case 1:
                        op = OpCodes.Ldarg_2;
                        break;
                    case 2:
                        op = OpCodes.Ldarg_3;
                        break;
                }
                genGet.Emit(OpCodes.Ldarg_0);
                if (i < 3)
                    genGet.Emit(op);
                else
                {
                    genGet.Emit(op, i + 1);
                    //for (int ai = 3; ai < fieldInfos.Count; ai++)
                    //    genGet.Emit(OpCodes.Ldarg_S, ai + 2);
                }
                genGet.Emit(OpCodes.Stfld, fieldInfos[i]);
            }
            genGet.Emit(OpCodes.Nop);
            genGet.Emit(OpCodes.Ret);
        }

        FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] fields = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty dp = properties[i];
                FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);

                MethodBuilder mbGet = tb.DefineMethod("get_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    dp.Type, Type.EmptyTypes);
                ILGenerator genGet = mbGet.GetILGenerator();
                genGet.Emit(OpCodes.Ldarg_0);
                genGet.Emit(OpCodes.Ldfld, fb);
                genGet.Emit(OpCodes.Ret);

                MethodBuilder mbSet = tb.DefineMethod("set_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new Type[] { dp.Type });
                ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }
            return fields;
        }

        void GenerateEquals(TypeBuilder tb, FieldInfo[] fields)
        {
            MethodBuilder mb = tb.DefineMethod("Equals",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(bool), new Type[] { typeof(object) });
            ILGenerator gen = mb.GetILGenerator();
            LocalBuilder other = gen.DeclareLocal(tb);
            Label next = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, tb);
            gen.Emit(OpCodes.Stloc, other);
            gen.Emit(OpCodes.Ldloc, other);
            gen.Emit(OpCodes.Brtrue_S, next);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new Type[] { ft, ft }), null);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
            }
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret);
        }

        void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields)
        {
            MethodBuilder mb = tb.DefineMethod("GetHashCode",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(int), Type.EmptyTypes);
            ILGenerator gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new Type[] { ft }), null);
                gen.Emit(OpCodes.Xor);
            }
            gen.Emit(OpCodes.Ret);
        }
    }

    internal class Signature : IEquatable<Signature>
    {
        public DynamicProperty[] Properties;
        public readonly int HashCode;

        public Signature(IEnumerable<DynamicProperty> properties)
        {
            var dynamicProperties = properties as DynamicProperty[] ?? properties.ToArray();

            Properties = dynamicProperties.ToArray();
            HashCode = 0;
            foreach (var p in dynamicProperties)
            {
                HashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature && Equals((Signature)obj);
        }

        public bool Equals(Signature other)
        {
            if (Properties.Length != other.Properties.Length) return false;
            for (var i = 0; i < Properties.Length; i++)
            {
                if (Properties[i].Name != other.Properties[i].Name ||
                    Properties[i].Type != other.Properties[i].Type) return false;
            }
            return true;
        }
    }

    public abstract class DynamicClass
    {
        public DynamicClass() { }

        public override string ToString()
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return string.Join(",", props.Select(o => o.GetValue(this, null) + "").ToArray());
            //var sb = new StringBuilder();
            //sb.Append("{");
            //for (int i = 0; i < props.Length; i++)
            //{
            //    if (i > 0) sb.Append(", ");
            //    sb.Append(props[i].Name);
            //    sb.Append("=");
            //    sb.Append(props[i].GetValue(this, null));
            //}
            //sb.Append("}");
            //return sb.ToString();
        }
    }

    public class DynamicProperty
    {
        readonly string _name;
        readonly Type _type;

        public DynamicProperty(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            _name = name;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _type; }
        }
    }
}
