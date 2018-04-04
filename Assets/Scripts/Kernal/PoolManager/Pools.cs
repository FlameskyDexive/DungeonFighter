using UnityEngine;
using System.Collections.Generic;

namespace Kernal
{
    /// <summary>
    /// ��ģ����ع�����
    /// </summary>
    public class Pools : MonoBehaviour
    {
        public List<PoolOption> PoolOptionLib = new List<PoolOption>();
        public bool IsUsedTime = false;
        private Transform ThisGameObjectPosition;

        void Awake()
        {
            PoolManager.Add(this);
            ThisGameObjectPosition = transform;
            PreLoadGameObject();
        }

        void Start()
        {
            if (IsUsedTime)
            {
                InvokeRepeating("ProcessGameObject_NameTime", 1F, 10F);
            }
        }

        /// <summary>
        /// ʱ�������
        /// ��Ҫҵ���߼�:
        /// 1>�� ÿ���10���֣�����������ʹ�õĻ״̬��Ϸ�����ʱ�����ȥ10�롣
        /// 2>:  ���ÿ���״̬����Ϸ�������Ƶ�ʱ������С�ڵ���0����������״̬��
        /// 3>:  ���½���״̬����Ϸ���󣬻��Ԥ���趨�Ĵ��ʱ��д��������Ƶ�ʱ����С�
        /// </summary>
        void ProcessGameObject_NameTime()
        {
            for (int i = 0; i < PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                //��������ʹ�õĻ״̬��Ϸ�����ʱ�����ȥ10��
                //���ÿ���״̬����Ϸ�������Ƶ�ʱ������С�ڵ���0����������״̬
                opt.AllActiveGameObjectTimeSubtraction();
            }
        }

        public void PreLoadGameObject()
        {
            for (int i = 0; i < this.PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                for (int j = opt.totalCount; j < opt.preLoadNumber; j++)
                {
                    GameObject obj = opt.PreLoad(opt.Prefab, Vector3.zero, Quaternion.identity);
                    obj.transform.parent = ThisGameObjectPosition;
                }
            }
        }

        /// <summary>
        ///  �õ���Ϸ���󣬴ӻ�����У�����ģ�����ϣ�
        /// 
        /// ���������� 
        ///     1�� ��ָ����Ԥ�衱���Լ��Ļ�����м���һ�����Ҽ����Լ�������е�"���ü����"��
        ///     2�� Ȼ���ٽ���һ���ض����Ҽ���Ԥ�裬�ټ����Լ��Ļ�����еġ����ü���ء��С�
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public GameObject GetGameObjectByPool(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            GameObject obj = null;

            for (int i = 0; i < PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                if (opt.Prefab == prefab)
                {
                    obj = opt.Active(pos, rot);
                    if (obj == null) return null;

                    if (obj.transform.parent != ThisGameObjectPosition)
                    {
                        obj.transform.parent = ThisGameObjectPosition;
                    }
                }
            }

            return obj;
        }

        public void RecoverGameObjectToPools(GameObject instance)
        {
            for (int i = 0; i < this.PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                if (opt.ActiveGameObjectArray.Contains(instance))
                {
                    if (instance.transform.parent != ThisGameObjectPosition)
                        instance.transform.parent = ThisGameObjectPosition;
                    opt.Deactive(instance);
                }
            }
        }

        public void DestoryUnused()
        {
            for (int i = 0; i < this.PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                opt.ClearUpUnused();
            }
        }

        public void DestoryPrefabCount(GameObject prefab, int count)
        {
            for (int i = 0; i < this.PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                if (opt.Prefab == prefab)
                {
                    opt.DestoryCount(count);
                    return;
                }
            }

        }

        public void OnDestroy()
        {
            if (IsUsedTime)
            {
                CancelInvoke("ProcessGameObject_NameTime");
            }
            for (int i = 0; i < this.PoolOptionLib.Count; i++)
            {
                PoolOption opt = this.PoolOptionLib[i];
                opt.ClearAllArray();
            }
        }

    }


    /// <summary>
    /// ��Ϸ�������ͣ�����������������ģ�ز�������
    ///          ���ܣ� ����ջء�Ԥ���صȡ�
    /// </summary>
    [System.Serializable]
    public class PoolOption
    {
        public GameObject Prefab;
        public int preLoadNumber = 0;
        public int autoDeactiveGameObjectByTime = 30;

        [HideInInspector]
        public List<GameObject> ActiveGameObjectArray = new List<GameObject>();
        [HideInInspector]
        public List<GameObject> InactiveGameObjectArray = new List<GameObject>();
        private int _Index = 0;


        internal GameObject PreLoad(GameObject prefab, Vector3 positon, Quaternion rotation)
        {
            GameObject obj = null;

            if (prefab)
            {
                obj = Object.Instantiate(prefab, positon, rotation) as GameObject;
                Rename(obj);
                obj.SetActive(false);
                InactiveGameObjectArray.Add(obj);
            }
            return obj;
        }

        internal GameObject Active(Vector3 pos, Quaternion rot)
        {
            GameObject obj;

            if (InactiveGameObjectArray.Count != 0)
            {
                obj = InactiveGameObjectArray[0];
                InactiveGameObjectArray.RemoveAt(0);
            }
            else
            {
                obj = Object.Instantiate(Prefab, pos, rot) as GameObject;
                Rename(obj);
            }
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            ActiveGameObjectArray.Add(obj);
            obj.SetActive(true);

            return obj;
        }

        internal void Deactive(GameObject obj)
        {
            ActiveGameObjectArray.Remove(obj);
            InactiveGameObjectArray.Add(obj);
            obj.SetActive(false);
        }

        internal int totalCount
        {
            get
            {
                int count = 0;
                count += this.ActiveGameObjectArray.Count;
                count += this.InactiveGameObjectArray.Count;
                return count;
            }
        }

        internal void ClearAllArray()
        {
            ActiveGameObjectArray.Clear();
            InactiveGameObjectArray.Clear();
        }

        internal void ClearUpUnused()
        {
            foreach (GameObject obj in InactiveGameObjectArray)
            {
                Object.Destroy(obj);
            }

            InactiveGameObjectArray.Clear();
        }

        /// <summary>
        /// ��Ϸ����������
        /// ���²�������Ϸ������ͳһ��ʽ����Ŀ��������ʱ���������
        /// </summary>
        /// <param name="instance"></param>    
        private void Rename(GameObject instance)
        {
            instance.name += (_Index + 1).ToString("#000");
            //��Ϸ�����Զ����ã�ʱ���  [Adding]
            instance.name = autoDeactiveGameObjectByTime + "@" + instance.name;
            _Index++;
        }

        internal void DestoryCount(int count)
        {
            if (count > InactiveGameObjectArray.Count)
            {
                ClearUpUnused();
                return;
            }
            for (int i = InactiveGameObjectArray.Count - 1; i >= InactiveGameObjectArray.Count - count; i--)
            {

                Object.Destroy(InactiveGameObjectArray[i]);
            }
            InactiveGameObjectArray.RemoveRange(InactiveGameObjectArray.Count - count, count);
        }

        /// <summary>
        /// �ص�������ʱ�������
        /// ���ܣ�������Ϸ�������ʱ�䵹��ʱ����ʱ��С��������С��ǻ�����������У���:��ʱ���Զ�������Ϸ����
        /// </summary>
        internal void AllActiveGameObjectTimeSubtraction()
        {
            for (int i = 0; i < ActiveGameObjectArray.Count; i++)
            {
                string strHead = null;
                string strTail = null;
                int intTimeInfo = 0;
                GameObject goActiveObj = null;

                goActiveObj = ActiveGameObjectArray[i];
                //�õ�ÿ�������ʱ���
                string[] strArray = goActiveObj.name.Split('@');
                strHead = strArray[0];
                strTail = strArray[1];

                //ʱ���-10 ����
                intTimeInfo = System.Convert.ToInt32(strHead);
                if (intTimeInfo >= 10)
                {
                    strHead = (intTimeInfo - 10).ToString();
                }
                else if (intTimeInfo <= 0)
                {
                    goActiveObj.name = autoDeactiveGameObjectByTime.ToString() + "@" + strTail;
                    this.Deactive(goActiveObj);
                    continue;
                }
                //ʱ�����������
                goActiveObj.name = strHead + '@' + strTail;
            }
        }

    }//PoolOption.cs_end


    /// <summary>
    /// �ڲ��ࣺ ��ʱ��
    /// </summary>
    //[System.Serializable]
    public class PoolTimeObject
    {
        public GameObject instance;
        public float time;
    }
}
