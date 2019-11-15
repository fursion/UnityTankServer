using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using LockStepServer1._0.Protocol;

namespace LockStepServer1._0.Core
{
    public enum FriendOpType
    {
        Add,
        Delete,
        BlackList,
        Apply
    }
    class DataMgr
    {
        MySqlConnection sqlconn;
        public static DataMgr instance;
        public DataMgr()
        {
            instance = this;
            Connect();
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        public void Connect()
        {
            string connStr = "Database=TankTest;DataSource=cdb-ahtsamo2.cd.tencentcdb.com;";
            connStr += "User ID=root;Password=Dj199706194430;port=10000;";
            sqlconn = new MySqlConnection(connStr);
            try
            {
                sqlconn.Open();
                Console.WriteLine("[DataMgr] Connected!");
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] Connect :" + e.Message);
                return;
            }
        }
        public bool IsSafeStr(string str)//正则表达式
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|{|%|@|\*]");
        }
        public bool CanRegister(string NickName)
        {
            if (!IsSafeStr(NickName))
                return false;
            string cmdStr = string.Format("select * from User where NickName='{0}';", NickName);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                bool hasRows = reader.HasRows;
                reader.Close();
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CanRegister :" + e.Message);
                return false;
            }
        }
        public int UserCount()
        {
            string CMDstr = "select count(*) from User";
            MySqlCommand cmd = new MySqlCommand(CMDstr, sqlconn);
            try
            {
                string UserCount = cmd.ExecuteScalar().ToString();
                int count = Convert.ToInt32(UserCount);
                return count;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }
        //注册    
        public Tuple<bool, long> Registe(string NickName, string PW)
        {
            if (!IsSafeStr(NickName) || !IsSafeStr(PW))
            {
                Console.WriteLine("[DataMgr] Register : 使用非法字符！");
                Tuple<bool, long> tup = new Tuple<bool, long>(false, 0);
                return tup;
            }
            if (!CanRegister(NickName))
            {
                Console.WriteLine("[DataMgr] Register: 昵称已被使用！");
                Tuple<bool, long> tup = new Tuple<bool, long>(false, 0);
                return tup;
            }
            long PlayerID = ModPlayerID(MadePlayerID());
            string cmdStr = string.Format("insert into User set NickName='{0}',PassWord='{1}',PlayerID='{2}';", NickName, PW, PlayerID);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("注册成功");
                Tuple<bool, long> tup = new Tuple<bool, long>(true, PlayerID);
                return tup;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] Register :" + e.Message);
                Tuple<bool, long> tup = new Tuple<bool, long>(false, 0);
                return tup;
            }
        }
        public bool Register(string Openid, string NickName, UserData UD)
        {
            string cmdStr = string.Format("insert into User set Openid='{0}',NickName='{1}';", Openid, NickName);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            try
            {
                cmd.ExecuteNonQuery();
                CreatPlayer(NickName, Openid, UD);
                Console.WriteLine("注册成功");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] Register :" + e.Message);
                return false;
            }
        }
        public long ModPlayerID(long PlayerID)
        {
            if (!ChackPlayerID(PlayerID))
            {
                PlayerID++;
                ModPlayerID(PlayerID);
            }
            return PlayerID;
        }
        public bool ChackPlayerID(long PlayerID)
        {
            string sqlstr = string.Format("select * from User where PlayerID='{0}';", PlayerID);
            MySqlCommand cmd = new MySqlCommand(sqlstr, sqlconn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                bool HasRows = reader.HasRows;
                reader.Close();
                return !HasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }
        public long MadePlayerID()
        {
            string NowTime = DateTime.Now.ToString("yyyyMMdd");
            Console.WriteLine(NowTime);
            int usercount = DataMgr.instance.UserCount();
            if (usercount < 0)
                return -1;
            string PID = NowTime + usercount.ToString().PadLeft(8, '0');
            long PlayerID = Convert.ToInt64(PID);
            return PlayerID;
        }
        //创建角色
        public bool CreatPlayer(string NickName, string Openid, UserData UD)
        {
            if (!IsSafeStr(Openid))
                return false;
            //序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            playerData.Openid = Openid;
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CreatPlayer :" + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            MemoryStream memory = new MemoryStream();
            Friend friend = new Friend();
            formatter.Serialize(memory, friend);
            byte[] friendArr = memory.ToArray();
            MemoryStream UDStream = new MemoryStream();
            formatter.Serialize(UDStream, UD);
            byte[] UDArr = UDStream.ToArray();
            //写入数据库 
            string cmdStr = string.Format("insert into Player set ID='{0}',Data=@Data,Friend=@Friend,UserData=@UD ;", Openid);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            cmd.Parameters.Add("@Data", MySqlDbType.Blob);
            cmd.Parameters.Add("@Friend", MySqlDbType.Blob);
            cmd.Parameters.Add("@UD", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            cmd.Parameters[1].Value = friendArr;
            cmd.Parameters[2].Value = UDArr;
            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("写入" + Openid);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CreatPlayer 写入" + e.Message);
                return false;
            }
        }
        // public bool Friend()
        //检测用户名和密码
        public bool CheckPassWordAndId(string NickName, string PW)
        {
            if (!IsSafeStr(NickName) || !IsSafeStr(PW))
                return false;
            string cmdStr = string.Format("select * from User where NickName='{0}' and PassWord='{1}';", NickName, PW);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                bool HasRows = reader.HasRows;
                reader.Close();
                return HasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CheckPassWordAndId :" + e.Message);
                return false;
            }
        }
        public bool CheckOpenid(string Openid)
        {
            string cmdStr = string.Format("select * from User where Openid='{0}';", Openid);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                bool HasRows = reader.HasRows;
                reader.Close();
                return HasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CheckOpenid :" + e.Message);
                return false;
            }
        }
        //获取角色数据
        public PlayerData GetPlayerData(string Openid)
        {
            PlayerData playerData = null;
            //if (!IsSafeStr(Openid))
            //    return playerData;
            string cmdStr = string.Format("select * from Player where ID='{0}';", Openid);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return playerData;
                }
                reader.Read();
                long len = reader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                reader.GetBytes(1, 0, buffer, 0, (int)len);
                reader.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] GetPlayerData 查询" + e.Message);
                return playerData;
            }
            //反序列化
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] GetPlayerData 反序列化" + e.Message);
                return playerData;
            }
        }
        //保存角色数据
        public bool SavePlayer(Player player)
        {
            string ID = player.Name;
            PlayerData playerData = player.data;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化" + e.Message);
                return false;
            }
            byte[] bytes = stream.ToArray();
            string formatStr = string.Format("update Player set data=@data Where ID='{0}';", ID);
            MySqlCommand cmd = new MySqlCommand(formatStr, sqlconn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = bytes;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer " + e.Message);
                return false;
            }

        }
        public UserData GetUserData(string Openid)
        {
            UserData UD = null;
            string cmdStr = string.Format("select * from Player where ID='{0}';", Openid);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return UD;
                }
                reader.Read();
                long len = reader.GetBytes(3, 0, null, 0, 0);
                buffer = new byte[len];
                reader.GetBytes(3, 0, buffer, 0, (int)len);
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] GetUserData" + e.Message);
                return UD;
            }
            //反序列化
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                UD = (UserData)formatter.Deserialize(stream);
                return UD;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] GetUserData反序列化" + e.Message);
                return UD;
            }
       }
        public Dictionary<string, UserData> FindUser(string NickName)
        {
            Dictionary<string, UserData> UserList = new Dictionary<string, UserData>();
            string find = string.Format("select * from User where NickName='{0}';", NickName);
            MySqlCommand cmd = new MySqlCommand(find, sqlconn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                return UserList;
            }
            else
            {
                List<string> List = new List<string>();
                while (reader.Read())
                {
                    List.Add(reader[2].ToString());
                }
                reader.Close();
                for (int i = 0; i < List.Count; i++)
                {
                    string cmdStr = string.Format("select * from Player where ID='{0}';", List[i]);
                    MySqlCommand Ucmd = new MySqlCommand(cmdStr, sqlconn);
                    MySqlDataReader Ureader = Ucmd.ExecuteReader();
                    if (!Ureader.HasRows)
                    {
                        continue;
                    }
                    else
                    {
                        Ureader.Read();
                        long len = Ureader.GetBytes(3, 0, null, 0, 0);
                        byte[] buffer = new byte[len];
                        Ureader.GetBytes(3, 0, buffer, 0, (int)len);
                        MemoryStream stream = new MemoryStream(buffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        UserData Usd = new UserData();
                        Usd = (UserData)formatter.Deserialize(stream);
                        UserList.Add(List[i], Usd);
                    }
                    Ureader.Close();
                }
                return UserList;
            }
        }
        public Friend GetFriend(string Openid)
        {
            try
            {
                Friend friend = null;
                string cmdStr = string.Format("select * from Player where ID='{0}';", Openid);
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlconn);
                byte[] buffer = new byte[1];
                try
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        reader.Close();
                        return friend;
                    }
                    reader.Read();
                    long len = reader.GetBytes(2, 0, null, 0, 0);
                    buffer = new byte[len];
                    reader.GetBytes(2, 0, buffer, 0, (int)len);
                    reader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[DataMgr] GetFriend" + e.Message);
                    return friend;
                }
                //反序列化
                MemoryStream stream = new MemoryStream(buffer);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    friend = (Friend)formatter.Deserialize(stream);
                    return friend;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[DataMgr] GetFriend 反序列化" + e.Message);
                    return friend;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetFriend 反序列化" + e.Message);
                return null;
            }

        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool AddFriend(Player player, string TargetOpenid)
        {
            string Openid = player.Openid;
            FriendInfo OneselfInfo = new FriendInfo
            {
                data = player.UserData
            };
            FriendInfo TarInfo = new FriendInfo
            {
                data = GetUserData(TargetOpenid)
            };
            try
            {
                Friend friend = GetFriend(Openid);
                Friend Tarfriend = GetFriend(TargetOpenid);
                bool TG = Tarfriend.GoodList.ContainsKey(TargetOpenid);
                bool G = friend.GoodList.ContainsKey(Openid);
                bool TA = friend.ApplyList.ContainsKey(TargetOpenid);
                if (!TA)
                    return false;
                if (G)
                {
                    if (!TG)
                        Tarfriend.GoodList.Add(Openid, OneselfInfo);
                    else
                        return true;
                }
                else
                {
                    if (TG)
                        friend.GoodList.Add(TargetOpenid, TarInfo);
                    else
                    {
                        friend.GoodList.Add(TargetOpenid, TarInfo);
                        Tarfriend.GoodList.Add(Openid, TarInfo);
                    }
                }
                friend.ApplyList.Remove(TargetOpenid);
                string Fstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                MySqlCommand command = new MySqlCommand(Fstr, sqlconn);
                command.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] Fbyte = Serialize(friend);
                command.Parameters[0].Value = Fbyte;
                command.ExecuteNonQuery();
                string Tstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", TargetOpenid);
                MySqlCommand TarCom = new MySqlCommand(Tstr, sqlconn);
                TarCom.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] TarByte = Serialize(Tarfriend);
                TarCom.Parameters[0].Value = TarByte;
                TarCom.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

        }
        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool DelFriend(string Openid, string TargetOpenid)
        {
            try
            {
                Friend friend = GetFriend(Openid);
                Friend Tarfriend = GetFriend(TargetOpenid);
                bool T = Tarfriend.GoodList.ContainsKey(Openid);
                bool F = friend.GoodList.ContainsKey(TargetOpenid);
                if (T)
                    Tarfriend.GoodList.Remove(Openid);
                if (F)
                    friend.GoodList.Remove(TargetOpenid);
                string Fstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                MySqlCommand command = new MySqlCommand(Fstr, sqlconn);
                command.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] Fbyte = Serialize(friend);
                command.Parameters[0].Value = Fbyte;
                command.ExecuteNonQuery();
                string Tstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", TargetOpenid);
                MySqlCommand TarCom = new MySqlCommand(Tstr, sqlconn);
                TarCom.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] TarByte = Serialize(Tarfriend);
                TarCom.Parameters[0].Value = TarByte;
                TarCom.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        /// <summary>
        /// 加入黑名单
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool AddBlackFriend(Player player, string TargetOpenid)
        {
            string Openid = player.Openid;
            FriendInfo TarInfo = new FriendInfo
            {
                data = GetUserData(TargetOpenid)
            };
            try
            {
                Friend friend = GetFriend(Openid);
                bool F = friend.GoodList.ContainsKey(TargetOpenid);
                bool B = friend.BlackList.ContainsKey(TargetOpenid);
                if (B)
                    return false;
                friend.BlackList.Add(TargetOpenid, TarInfo);
                string Fstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                MySqlCommand command = new MySqlCommand(Fstr, sqlconn);
                command.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] Fbyte = Serialize(friend);
                command.Parameters[0].Value = Fbyte;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        /// <summary>
        /// 移出黑名单
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool DelBlackFriend(string Openid, string TargetOpenid)
        {
            try
            {

                Friend friend = GetFriend(Openid);
                bool F = friend.GoodList.ContainsKey(TargetOpenid);
                bool B = friend.BlackList.ContainsKey(TargetOpenid);
                if (!B)
                    return false;
                friend.BlackList.Remove(TargetOpenid);
                string Fstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                MySqlCommand command = new MySqlCommand(Fstr, sqlconn);
                command.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] Fbyte = Serialize(friend);
                command.Parameters[0].Value = Fbyte;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        /// <summary>
        /// 加入申请列表
        /// Openid是请求者，TargetOpenid是被请求者
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool AddApplyFriend(Player player, string TargetOpenid)
        {
            string Openid = player.Openid;
            FriendInfo OnselfInfo = new FriendInfo
            {
                data = player.UserData
            };
            try
            {
                Friend friend = GetFriend(Openid);
                Friend Tarfriend = GetFriend(TargetOpenid);
                try
                {

                    bool TG = Tarfriend.GoodList.ContainsKey(Openid);
                    bool G = friend.GoodList.ContainsKey(TargetOpenid);
                    bool B = friend.BlackList.ContainsKey(TargetOpenid);
                    bool TB = Tarfriend.BlackList.ContainsKey(Openid);
                    bool TA = Tarfriend.ApplyList.ContainsKey(Openid);
                    if (TB || TA || G)
                        return false;
                    if (B)
                    {
                        try
                        {
                            friend.BlackList.Remove(TargetOpenid);
                            string Fstr = string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                            MySqlCommand command = new MySqlCommand(Fstr, sqlconn);
                            command.Parameters.Add("@Friend", MySqlDbType.Blob);
                            byte[] Fbyte = Serialize(friend);
                            command.Parameters[0].Value = Fbyte;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("AddApplyFriend  openid" + e.Message);
                            return false;
                        }
                    }
                    try
                    {

                        Tarfriend.ApplyList.Add(Openid, OnselfInfo);
                        string Tstr = string.Format("update Player set Friend=@Friend Where ID='{0}'", TargetOpenid);
                        MySqlCommand TarCom = new MySqlCommand(Tstr, sqlconn);
                        TarCom.Parameters.Add("@Friend", MySqlDbType.Blob);
                        byte[] TarByte = Serialize(Tarfriend);
                        TarCom.Parameters[0].Value = TarByte;
                        TarCom.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("AddApplyFriend  TargetOpenid" + e.Message);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("BOOL " + e.Message);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("AddApplyFriend  : " + e.Message);
                return false;
            }
        }
        /// <summary>
        /// 取消申请
        /// </summary>
        /// <param name="Openid"></param>
        /// <param name="TargetOpenid"></param>
        public bool DelApplyFriend(string Openid, string TargetOpenid)
        {
            try
            {
                //Friend Tarfriend = GetFriend(TargetOpenid);
                //bool TA = Tarfriend.ApplyList.ContainsKey(Openid);
                //if (!TA)
                //    return false;
                //Tarfriend.ApplyList.Remove(Openid);
                //string Tstr = string.Format("update Player set Friend=@Friend Where ID={0};", TargetOpenid);
                //MySqlCommand TarCom = new MySqlCommand(Tstr, sqlconn);
                //TarCom.Parameters.Add("@Friend", MySqlDbType.Blob);
                //byte[] TarByte = Serialize(Tarfriend);
                //TarCom.Parameters[0].Value = TarByte;
                //TarCom.ExecuteNonQuery();
                //return true;
                Friend MyFriend = GetFriend(Openid);
                bool A = MyFriend.ApplyList.ContainsKey(TargetOpenid);
                if (!A)
                    return true;
                MyFriend.ApplyList.Remove(TargetOpenid);
                string MyStr= string.Format("update Player set Friend=@Friend Where ID='{0}';", Openid);
                MySqlCommand MyCmd = new MySqlCommand(MyStr, sqlconn);
                MyCmd.Parameters.Add("@Friend", MySqlDbType.Blob);
                byte[] MyByte= Serialize(MyFriend);
                MyCmd.Parameters[0].Value = MyByte;
                MyCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

        }
        public byte[] Serialize(object o)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream memory = new MemoryStream();
            formatter.Serialize(memory, o);
            byte[] Arr = memory.ToArray();
            return Arr;
        }
        public object DeSerialize<T>(byte[] B)
        {
            MemoryStream stream = new MemoryStream(B);
            BinaryFormatter formatter = new BinaryFormatter();
            T buff = (T)formatter.Deserialize(stream);
            return buff;
        }
        public bool ListCheck(List<string> list, string TargetOpenid)
        {
            int count = list.Count;
            foreach (string s in list)
            {
                if (s == TargetOpenid)
                    return true;
            }
            return false;
        }
    }
}
