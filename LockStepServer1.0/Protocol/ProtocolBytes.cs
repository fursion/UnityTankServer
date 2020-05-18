using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace LockStepServer1._0.Protocol
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Fursion_Protocol
    {
        #region
        CheckOpenid, CreatateTeam,
        #endregion
        #region
        ExiteTeam, JoinTeam, RightUpDate,
        #endregion
        LoadingUpDate, GoToSelect
    }
    public class ProtocolBytes : ProtocolBase
    {
        public Fursion_Protocol Protocol { get; set; }
        private byte[] _bytes;
        public byte[] bytes
        {
            get { return _bytes; }
            set
            {
                _bytes = value;
                if (_bytes.Length <=13)
                    return;
                Int32 len = 13 + sizeof(Int32) + BitConverter.ToInt32(_bytes, 13) + sizeof(Int32);
                string Str = Encoding.UTF8.GetString(_bytes, len, BitConverter.ToInt32(_bytes, len - sizeof(Int32)));
                Protocol = JsonConvert.DeserializeObject<Fursion_Protocol>(Str);
            }
        }
        /// <summary>
        /// 解码接收到的数据
        /// </summary>
        /// <param name="readbuff"></param>
        /// <param name="start"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public override ProtocolBase Decode(byte[] readbuff, int start, int lenght)
        {
            ProtocolBytes protoco = new ProtocolBytes();
            protoco.bytes = new byte[lenght];
            Array.Copy(readbuff, start, protoco.bytes, 0, lenght);
            return protoco;
        }
        public override byte[] Encode()
        {
            return bytes;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string GetDesc()
        {
            string str = "";
            if (bytes == null) return str;
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = (int)bytes[i];
                str += b.ToString() + " ";
            }
            return str;
        }
        public void ModifyProtocolType(Fursion_Protocol protocol)
        {
            if (bytes == null || bytes.Length == 0)
            {
                this.AddData(JsonConvert.SerializeObject(protocol));
                return;
            }
            Int32 len = sizeof(Int32) + BitConverter.ToInt32(bytes, 0) + sizeof(Int32);
            byte[] CountBytes = bytes.Skip(0).Take(len).ToArray();

            string ProtocolStr = JsonConvert.SerializeObject(protocol);
            byte[] ModifyBytes = BitConverter.GetBytes(ProtocolStr.GetType().ToString().Substring(7).Length);
            ModifyBytes = ModifyBytes.Concat(Encoding.UTF8.GetBytes((ProtocolStr.GetType().ToString().Substring(7))).Concat(BitConverter.GetBytes(ProtocolStr.Length)).Concat(Encoding.UTF8.GetBytes(ProtocolStr))).ToArray(); ;


            Int32 Typelen = BitConverter.ToInt32(bytes, len);
            if (bytes.Length < sizeof(Int32) * 3 + Typelen + len + BitConverter.ToInt32(bytes, len + sizeof(Int32) + Typelen))
            {
                bytes = CountBytes.Concat(ModifyBytes).ToArray();
                return;
            }
            Int32 Protocollen = sizeof(Int32) * 2 + Typelen + BitConverter.ToInt32(bytes, len + sizeof(Int32) + Typelen);
            byte[] DateBytes = bytes.Skip(len + Protocollen).Take(bytes.Length - (len + Protocollen)).ToArray();
            bytes = CountBytes.Concat(ModifyBytes).Concat(DateBytes).ToArray();
        }
    }
    public static class AIDecod
    {
        private static byte[] bytes;
        private static bool tp;
        public static object[] GetDecode(this ProtocolBytes T)
        {
            Int32 start = 0;
            Int32 end = 0;
            return GetDecode(T, start, ref end);
        }
        public static void GetDecode(this byte[] vs)
        {
            Int32 len = vs.Length;
            string d = "";
            for (int i = 0; i < len; i++)
            {
                d = d + " " + vs[i].ToString();
            }
        }
        public static object Decount(this ProtocolBytes T, Int32 start, ref int end)
        {
            Int32 typelen = BitConverter.ToInt32(T.bytes, start);
            if (T.bytes.Length < sizeof(Int32) + typelen)
                return 0;
            string typen = Encoding.UTF8.GetString(T.bytes, sizeof(Int32), typelen);
            start = sizeof(Int32) + typelen;
            string methodname = "Get" + typen;
            Type tag = typeof(AIDecod);
            MethodInfo method = tag.GetMethod(methodname);
            if (method == null)
                return 0;
            object[] vs = { T.bytes, start, end };//添加函数参数
            object d = method.Invoke(tag, vs);
            end = (Int32)vs[2];
            return d;
        }
        /// <summary>
        /// 解码 返回object数组
        /// </summary>
        /// <param name="T"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static object[] GetDecode(this ProtocolBytes T, Int32 start, ref Int32 end)
        {
            object[] da;
            List<object> list = new List<object>();
            Int32 count = (int)Decount(T, start, ref start);
            for (int i = 0; i < count; i++)
            {
                Int32 typelen = BitConverter.ToInt32(T.bytes, start);
                //Debug.Log(start + sizeof(Int32) + typelen);
                if (T.bytes.Length < start + sizeof(Int32) + typelen)
                    return da = new object[] { "M=>ERR" };
                string typen = Encoding.UTF8.GetString(T.bytes, start + sizeof(Int32), typelen);
                start = start + sizeof(Int32) + typelen;
                string methodname = "Get" + typen;
                Type tag = typeof(AIDecod);
                MethodInfo method = tag.GetMethod(methodname);
                if (method == null)
                    return da = new object[] { 2 };
                object[] vs = { T.bytes, start, start };//添加函数参数
                object d = method.Invoke(tag, vs);
                start = end = (Int32)vs[2];
                list.Add(d);
            }
            return list.ToArray();

        }
        public static bool AddData(this ProtocolBytes T, object ob)
        {
            bytes = T.bytes;
            Int32 indexlen = 0;
            tp = false;
            if (T.bytes == null)
            {
                string typename = "Int32";
                byte[] count;
                Int32 Typelen = typename.Length;
                byte[] byteTypelen = BitConverter.GetBytes(Typelen);
                byte[] DataType = Encoding.UTF8.GetBytes(typename);
                byte[] MsgcountByte = BitConverter.GetBytes(0);
                count = byteTypelen.Concat(DataType).Concat(MsgcountByte).ToArray();
                indexlen = count.Length;
                T.bytes = count;
                T.bytes.GetDecode();
            }
            Int32 Msgcount = (int)T.Decount(0, ref indexlen);
            string type = ob.GetType().ToString();
            type = type.Substring(7); //移除system.
            Type tag = typeof(AIDecod);
            string methodname = "Add" + type;
            MethodInfo method = tag.GetMethod(methodname);
            if (method == null)
                return tp;//返回添加失败标志位
            object[] vs = { type, ob, T.bytes, Msgcount };//添加函数参数
            method.Invoke(tag, vs);
            T.bytes = (byte[])vs[2];
            Msgcount = (Int32)vs[3];
#if _DEBUG_
            Debug.Log("Msgcount b=" + Msgcount);
#endif
            UpdataCount(T.bytes, Msgcount, indexlen);//更新Msgcount
            T.bytes.GetDecode();
            return tp;
        }
        public static void AddString(string type, string data, ref byte[] targetbytes, ref Int32 Msgcount)
        {

            Int32 typelenght = type.Length;
            //Int32 datalenght = data.Length;
            byte[] tylenbyte = BitConverter.GetBytes(typelenght);//数据类型名长度
            byte[] typebyte = Encoding.UTF8.GetBytes(type);//数据类型
                                                           //byte[] datalenbyte = BitConverter.GetBytes(datalenght);
            byte[] databytes = Encoding.UTF8.GetBytes(data);
            Int32 datalenght = databytes.Length;
            byte[] datalenbyte = BitConverter.GetBytes(datalenght);
            if (targetbytes == null)
                targetbytes = tylenbyte.Concat(typebyte).Concat(datalenbyte).Concat(databytes).ToArray();
            else
                targetbytes = targetbytes.Concat(tylenbyte).Concat(typebyte).Concat(datalenbyte).Concat(databytes).ToArray();
            Msgcount++;
            tp = true;
        }
        public static byte[] AddSingle(string type, float data, ref byte[] targetbytes, ref Int32 Msgcount)
        {
            Int32 typelenght = type.Length;
            byte[] tylenbyte = BitConverter.GetBytes(typelenght);//数据类型名长度
            byte[] typebyte = Encoding.UTF8.GetBytes(type);//数据类型
            byte[] databyte = BitConverter.GetBytes(data);//数据
            if (targetbytes == null)
                targetbytes = tylenbyte.Concat(typebyte).Concat(databyte).ToArray();
            else
                targetbytes = targetbytes.Concat(tylenbyte).Concat(typebyte).Concat(databyte).ToArray();
            Msgcount++;
            tp = true;
            return targetbytes;
        }
        public static void AddInt64(string type, long data, ref byte[] targetbytes, ref Int32 Msgcount)
        {
            Int32 typelenght = type.Length;
            byte[] tylenbyte = BitConverter.GetBytes(typelenght);//数据类型名长度
            byte[] typebyte = Encoding.UTF8.GetBytes(type);//数据类型
            byte[] databyte = BitConverter.GetBytes(data);//数据
            if (targetbytes == null)
                targetbytes = tylenbyte.Concat(typebyte).Concat(databyte).ToArray();
            else
                targetbytes = targetbytes.Concat(tylenbyte).Concat(typebyte).Concat(databyte).ToArray();
            Msgcount++;
            tp = true;
        }
        public static Int64 GetInt64(byte[] vs, Int32 start, ref Int32 end)
        {
            Int64 data = BitConverter.ToInt64(vs, start);
            end = start + sizeof(Int32);
            return data;
        }
        public static void AddInt32(string type, int data, ref byte[] targetbytes, ref Int32 Msgcount)
        {

            Int32 typelenght = type.Length;
            byte[] tylenbyte = BitConverter.GetBytes(typelenght);//数据类型名长度
            byte[] typebyte = Encoding.UTF8.GetBytes(type);//数据类型
            byte[] databyte = BitConverter.GetBytes(data);//数据
            if (targetbytes == null)
                targetbytes = tylenbyte.Concat(typebyte).Concat(databyte).ToArray();
            else
                targetbytes = targetbytes.Concat(tylenbyte).Concat(typebyte).Concat(databyte).ToArray();
            Msgcount++;
            tp = true;
        }
        public static string GetString(byte[] vs, Int32 start, ref Int32 end)
        {
            Int32 len = BitConverter.ToInt32(vs, start);
            string da = Encoding.UTF8.GetString(vs, start + sizeof(Int32), len);
            end = start + sizeof(Int32) + len;
            return da;
        }
        public static void GetSingle()
        {

        }
        public static Int32 GetInt32(byte[] vs, Int32 start, ref Int32 end)
        {

            Int32 data = BitConverter.ToInt32(vs, start);
            end = start + sizeof(Int32);
#if _DEBUG_
            Debug.Log("GetInt32");
            vs.Decode();
#endif
            return data;
        }
        public static void GetCount(this byte[] vs)
        {

        }
        public static void UpdataCount(byte[] vs, Int32 count, Int32 indexlen)
        {
            string typename = "Int32";
            Int32 Typelen = typename.Length;
            byte[] coun = new byte[indexlen];
            byte[] byteTypelen = BitConverter.GetBytes(Typelen);
            byte[] DataType = Encoding.UTF8.GetBytes(typename);
            byte[] MsgcountByte = BitConverter.GetBytes(count);
            coun = byteTypelen.Concat(DataType).Concat(MsgcountByte).ToArray();
            vs.Replace(0, indexlen, coun, 0, indexlen);
        }
        /// <summary>
        /// ta为目标数组起始位，tb为终止位，t为资源数组，a为资源数组起始位，b为资源数组终止位
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="ta"></param>
        /// <param name="tb"></param>
        /// <param name="t"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Replace(this byte[] vs, int ta, int tb, byte[] t, int a, int b)
        {
            if ((tb - ta) != (b - a))
                return;
            int lenght = tb - ta;
            string s = "";
            for (int i = 0; i < lenght; i++)
            {
                vs[ta + i] = t[a + i];
                s += t[a + i].ToString();
            }
        }
        public static object StringDeSerialize<T>(this string Str)
        {
            try
            {
                T t = JsonConvert.DeserializeObject<T>(Str);
                return t;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static void PrintBytes(this object[] OB)
        {
            string s = "";
            for (int i = 0; i < OB.Length; i++)
            {
                s += OB[i].ToString();
            }
            Console.WriteLine(s);
        }
        public static void ColorWord(this string Text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(Text);
            Console.ResetColor();
        }
        public static string SerializableJSON(this object O)
        {
            return JsonConvert.SerializeObject(O);
        }
    }
}