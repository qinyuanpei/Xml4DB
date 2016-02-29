using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xml4DB;

namespace Xml4DBSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建数据库
            Xml4DB<Student> db = Xml4DB<Student>.Create("test.db");
            //插入一条数据
            db.Insert("001", new Student(001, "张三", 12));
            //提交更改
            db.Commit();

            //加载数据库
            db = Xml4DB<Student>.Load("test1.db");
            //删除ID为001的记录
            db.Delete("001");

            //插入三条数据
            db.Insert("001", new Student(001, "张三", 12));
            db.Insert("002", new Student(001, "李四", 20));
            db.Insert("003", new Student(001, "王五", 25));

            //更新ID为003的记录
            db.Update("003", new Student(001, "Wang5", 15));

            //提交更改
            db.Commit();

            //读取ID为002的记录
            Student stu = db.Read("002");
            
            //读取所有记录返回一个列表
            List<Student> studentList = db.ReadList();

            //读取所有记录返回一个数组
            Student[] students = db.Read();

            Console.ReadKey();
        }
    }

    /// <summary>
    /// 定义一个示例类Student
    /// </summary>
    public class Student
    {
        public Student()
        {

        }

        public Student(int mID, string mName, int mAge)
        {
            this.mID = mID;
            this.mName = mName;
            this.mAge = mAge;
        }

        private int mAge;
        public int Age
        {
            get { return mAge; }
            set { mAge = value; }
        }

        private int mID;
        public int ID
        {
            get { return mID; }
            set { mID = value; }
        }

        private string mName;
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public override string ToString()
        {
            return string.Format("ID={0},Name={1},Age={2}", this.ID, this.Name, this.Age);
        }
    }
}
