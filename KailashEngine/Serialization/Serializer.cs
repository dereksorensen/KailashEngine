using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace KailashEngine.Serialization
{
    class Serializer
    {

        private string _path_save_data;
        public string path_save_data
        {
            get { return _path_save_data; }
        }


        public Serializer(string base_data_path)
        {
            _path_save_data = base_data_path;
        }


        //------------------------------------------------------
        // Instanced Class Serialization
        //------------------------------------------------------

        // Save Instanced Class
        const int VERSION = 1;
        public void Save(object instanced_class, string fileName)
        {
            Stream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(_path_save_data + fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, VERSION);
                formatter.Serialize(stream, instanced_class);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }
        }

        // Load Instanced Class
        public object Load(string fileName)
        {
            Stream stream = null;
            object instanced_class = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(_path_save_data + fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                int version = (int)formatter.Deserialize(stream);
                System.Diagnostics.Debug.Assert(version == VERSION);
                instanced_class = formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("Cannot Load Data (" + fileName + ")", e.Message);
                return null;
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }
            return instanced_class;
        }


        //------------------------------------------------------
        // Static Class Serialization
        //------------------------------------------------------

        // Save Static Class
        public bool Save(Type static_class, string filename)
        {
            try
            {
                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);

                object[,] a = new object[fields.Length - 1, 2];
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType.ToString() == "game.Cycle") continue;
                    if (field.IsNotSerialized) continue;
                    a[i, 0] = field.Name;
                    a[i, 1] = field.GetValue(null);
                    i++;
                };
                Stream f = File.Open(_path_save_data + filename, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(f, a);
                f.Close();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString()); //Better error messages
                return false;
            }
        }

        // Load Static Class
        public bool Load(Type static_class, string filename)
        {
            try
            {
                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);
                object[,] a;
                Stream f = File.Open(_path_save_data + filename, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                a = formatter.Deserialize(f) as object[,];
                f.Close();
                if (a.GetLength(0) != fields.Length - 1) return false;

                foreach (FieldInfo field in fields)
                    for (int i = 0; i < fields.Length - 1; i++) //I ran into problems that some fields are dropped,now everyone is compared to everyone, problem fixed
                        if (field.Name == (a[i, 0] as string))
                            field.SetValue(null, a[i, 1]);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("FileNotFound"))
                {
                    System.Windows.Forms.MessageBox.Show("No " + static_class.Name + " Data Found. I will create one for you");
                    Save(static_class, _path_save_data + filename);
                    return true;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                    return false;
                }
            }
        }




    }
}
