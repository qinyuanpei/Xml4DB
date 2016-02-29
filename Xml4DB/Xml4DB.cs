using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xml4DB
{
	public class Xml4DB<T>
	{
		/// <summary>
		/// 当前数据类型
		/// </summary>
		private T target;

		/// <summary>
		/// 数据库路径
		/// </summary>
		private string dataPath;

		/// <summary>
		/// 当前类型的Xml映射
		/// </summary>
		private XElement elements;

		/// <summary>
		/// 构造函数
		/// </summary>
		private Xml4DB()
		{
			this.target = CreateInitiate();
			this.elements = new XElement(target.GetType().Name + "s");
		}
			
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="dataPath">数据库路径</param>
        private Xml4DB(string dataPath)
		{
			this.target = CreateInitiate();
			this.dataPath = dataPath;
			this.elements = LoadXml();
		}

        /// <summary>
        /// 创建数据库,如果文件存在则进行覆盖
        /// </summary>
        /// <param name="dataPath">数据库路径</param>
        /// <returns></returns>
        public static Xml4DB<T> Create(string dataPath)
        {
            Xml4DB<T> db = new Xml4DB<T>();
            db.dataPath = dataPath;
            db.Commit();
            return db;
        }

        /// <summary>
        /// 加载数据库,如果文件不存在则抛出异常
        /// </summary>
        /// <param name="dataPath">数据库路径</param>
        /// <returns></returns>
        public static Xml4DB<T> Load(string dataPath)
        {
            Xml4DB<T> db = new Xml4DB<T>(dataPath);
            return db;
        }

		/// <summary>
		/// 插入数据
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="t">T.</param>
		public void Insert(string id,T t)
		{
            if(elements == null)
                return;

			//将映射类型转化为xml结构
			XElement xe = ConvertTargetToXml(t);
			//设置唯一的ID
			xe.Add(new XAttribute("ID",id));
			//添加到Xml
			elements.Add(xe);
		}

		/// <summary>
		/// 删除数据
		/// </summary>
		/// <param name="id">唯一的ID</param>
		public void Delete(string id)
		{
            if(elements == null)
                return;

			//获取指定ID的记录
			IEnumerable<XElement> data = 
				from XmlDB in elements.Elements(target.GetType().Name)
					where XmlDB.Attribute("ID").Value == id
				select XmlDB;

			//取第一个元素作为筛选结果
			XElement xe = null;
			if(data.Count () > 0)
				xe = data.ElementAt(0);

			//移除元素
			xe.Remove();
		}

		/// <summary>
		/// 更新数据
		/// </summary>
		/// <param name="">唯一的ID</param>
		public void Update(string id,T t)
		{
            if(elements == null)
                return;

			//获取指定ID的记录
			IEnumerable<XElement> data = 
				from XmlDB in elements.Elements(target.GetType().Name)
					where XmlDB.Attribute("ID").Value == id
				select XmlDB;

			//取第一个元素作为筛选结果
			XElement xe = null;
			if(data.Count () > 0)
				xe = data.ElementAt(0);

			//更新元素
			UpdateElement(xe,t);
		}

		/// <summary>
		/// 读取数据
		/// </summary>
		/// <param name="id">唯一ID</param>
		public T Read(string id)
		{
            if(elements == null)
                return default(T);

			//获取指定ID的记录
			IEnumerable<XElement> sets = 
				from XmlDB in elements.Elements(target.GetType().Name)
					where XmlDB.Attribute("ID").Value == id
				select XmlDB;

			//取第一个元素作为筛选结果
			XElement xe = null;
			if(sets.Count () > 0)
				xe = sets.ElementAt(0);

			if(xe == null)
                return default(T);

			return ConvertXmlToTarget(xe);
		}


		/// <summary>
		/// 读取全部数据
		/// </summary>
		public T[] Read()
		{
            if(elements == null)
                return null;

			//获取指定ID的记录
			IEnumerable<XElement> sets = 
				from XmlDB in elements.Elements(target.GetType().Name)
				select XmlDB;

			if(sets.Count () == 0)
				return null;
			
			//遍历每一个元素将其映射为具体类型
			T[] data = new T[sets.Count ()];
			for(int i = 0; i < sets.Count (); i++) 
			{
				data [i] = ConvertXmlToTarget(sets.ElementAt (i));
			}

			return data;
		}

		/// <summary>
		/// 读取全部数据
		/// </summary>
		public List<T> ReadList()
		{
            if(elements == null)
                return null;

			//获取指定ID的记录
			IEnumerable<XElement> sets = 
				from XmlDB in elements.Elements(target.GetType().Name)
				select XmlDB;

			if(sets.Count () == 0)
				return null;

			//遍历每一个元素将其映射为具体类型
			List<T> data = new List<T>();
			for(int i = 0; i < sets.Count (); i++) 
			{
				data.Add(ConvertXmlToTarget (sets.ElementAt (i)));
			}

			return data;
		}

		/// <summary>
		/// 提交
		/// </summary>
		public void Commit()
		{
            if(elements == null)
                return;
			elements.Save(dataPath);
		}

		/// <summary>
		/// 可查询结果
		/// </summary>
		public IQueryable Query()
		{
			T[] data = Read();
			return data.AsQueryable ();
		}
			
		/// <summary>
		/// 创建当前类型实例
		/// </summary>
		/// <returns>The initiate.</returns>
		private T CreateInitiate()
		{
			Type t = typeof(T);
			ConstructorInfo ct = t.GetConstructor(System.Type.EmptyTypes);
			return (T)ct.Invoke(null);
		}

		/// <summary>
		/// 加载Xml文件
		/// </summary>
		/// <returns>The xml.</returns>
		private XElement LoadXml()
		{
            XElement data = null;

            //当文件不存在是抛出异常信息
            if(!File.Exists(dataPath))
                throw new FileNotFoundException("请确认数据文件存在!");

            if(dataPath != null && File.Exists(dataPath))
                data = XElement.Load(dataPath);

            return data;
		}


	    
		/// <summary>
		/// 将Xml文件转化为映射类型
		/// </summary>
		/// <returns>The xml to target.</returns>
		/// <param name="element">类型对应的xml映射</param>
		private T ConvertXmlToTarget(XElement element)
		{
			if(target != null)
                target = default(T);

			target = CreateInitiate();

			//如果Xml和类型无法对应则返回为空
			if(element.Name != target.GetType ().Name)
				return default(T);

			//获取类型
			Type mType = target.GetType();
			//获取属性集合
			PropertyInfo[] mPropertys = mType.GetProperties();

			//初始化属性名称
			string propertyName = string.Empty;

			//遍历属性
			foreach(PropertyInfo property in mPropertys)
			{
				if(element.Element(property.Name) != null) {
					property.SetValue(target, Convert.ChangeType(element.Element(property.Name).Value, property.PropertyType),null);
				}
			}

			return target;
		}

		/// <summary>
		/// 将映射类型转化为xml结构
		/// </summary>
		/// <returns>The target to xml.</returns>
		/// <param name="t">T.</param>
		private XElement ConvertTargetToXml(T t)
		{
			//获取类型
			Type mType = t.GetType();
			//获取属性集合
			PropertyInfo[] mPropertys = mType.GetProperties();
			//存储每个属性的object数组
			object[] mXEleObject = new object[mPropertys.Length];

		    //遍历属性
			for(int i = 0; i < mXEleObject.Length; i++)
			{
				//获取属性值
				object mValue = mPropertys[i].GetValue(t, null);
				if(mValue != null) {
					mXEleObject[i] = new XElement(mPropertys [i].Name, mValue.ToString());
				}else{
					mXEleObject[i] = new XElement(mPropertys [i].Name, "");
				}
			}

			return new XElement(t.GetType().Name, mXEleObject);
		}

		/// <summary>
		/// 更新Xml节点
		/// </summary>
		/// <param name="xe">Xml映射节点</param>
		/// <param name="t">T.</param>
		private void UpdateElement(XElement xe,T t)
		{
			//获取类型
			Type mType = t.GetType();
			//获取属性集合
			PropertyInfo[] mPropertys = mType.GetProperties();
			//遍历属性
			foreach(PropertyInfo property in mPropertys) 
			{
				//获取属性值
				object mValue = property.GetValue(t, null);
				if(mValue != null)
					xe.Element(property.Name).SetValue(mValue.ToString());
			}
		}
	}
}

