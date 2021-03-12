using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace Framework
{

    /// <summary>
    /// 场景切换模块
    /// </summary>
    public class ScenesMgr : SingletonBase<ScenesMgr>
    {
        Image Background = null;
        Text TextUI = null;

        /// <summary>
        /// 同步加载场景 容易卡顿 建议使用LoadSceneAsyn
        /// </summary>
        /// <param name="name">场景名称</param>
        /// <param name="function">切换场景后调用的函数，没有请传null</param>
        public void LoadScene(string name, UnityAction function)
        {
            //场景同步加载
            SceneManager.LoadScene(name);

            function?.Invoke(); //function不为空则执行
        }

        public void ReloadCurrentSceneAsyn(UnityAction function, UnityAction<float> ProgressUpdate)
        {
            if (Background == null || TextUI == null)
            {
                ResMgr.I().LoadAsync<GameObject>("UI/LoadingCanvas", (obj) =>
                {
                    DontDestroyOnLoad(obj);
                    Background = obj.GetComponentInChildren<Image>();
                    TextUI = obj.GetComponentInChildren<Text>();
                    StartCoroutine(LoadSceneAsynAction(SceneManager.GetActiveScene().name, function, ProgressUpdate));
                });
            }
            else
            {
                StartCoroutine(LoadSceneAsynAction(SceneManager.GetActiveScene().name, function, ProgressUpdate));
            }
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="name">场景名称</param>
        /// <param name="function">切换场景后调用的函数，没有请传null</param>
        /// /// <param name="ProgressUpdate">切换场景中的加载进度，没有请传null</param>
        public void LoadSceneAsyn(string name, UnityAction function, UnityAction<float> ProgressUpdate)
        {
            if (Background == null || TextUI == null)
            {
                ResMgr.I().LoadAsync<GameObject>("UI/LoadingCanvas", (obj) =>
                {
                    DontDestroyOnLoad(obj);
                    Background = obj.GetComponentInChildren<Image>();
                    TextUI = obj.GetComponentInChildren<Text>();
                    StartCoroutine(LoadSceneAsynAction(name, function, ProgressUpdate));
                });
            }
            else
            {
                StartCoroutine(LoadSceneAsynAction(name, function, ProgressUpdate));
            }
        }

        private IEnumerator LoadSceneAsynAction(string name, UnityAction funtion, UnityAction<float> ProgressUpdate)
        {
            //记录开始加载的时间
            float stime = Time.realtimeSinceStartup;
            //清空上一个场景记录的东西
            CachePool.I().Clear();
            EventCenter.I().Clear();

            //Fade in 加载UI
            DOTween.Kill(Background);
            DOTween.Kill(TextUI);
            Background.DOColor(new Color(Background.color.r, Background.color.g, Background.color.b, 1), 0.2f);
            TextUI.DOColor(new Color(TextUI.color.r, TextUI.color.r, TextUI.color.r, 1), 0.2f);
            //开始加载
            AsyncOperation AO = SceneManager.LoadSceneAsync(name);
            //进入加载循环
            while (!AO.isDone)
            {
                //进度条更新事件
                ProgressUpdate?.Invoke(AO.progress);
                //更新加载的进度显示
                TextUI.text = new StringBuilder("Loading: ").Append((AO.progress * 100).ToString()).Append("%").ToString();
                //做一个返回数值，主要用于将下一次循环放到下一帧再执行
                yield return AO.progress;
            }
            //结束加载循环
            TextUI.text = new StringBuilder("Loading: ").Append((AO.progress * 100).ToString()).Append("%").ToString();
            //Fade Out UI
            DOTween.Kill(Background);
            DOTween.Kill(TextUI);
            TextUI.DOColor(new Color(TextUI.color.r, TextUI.color.r, TextUI.color.r, 0), 0.2f);
            Background.DOColor(new Color(Background.color.r, Background.color.g, Background.color.b, 0), 0.4f);
            //触发加载完毕的委托
            funtion?.Invoke(); //function不为空则执行

            Debug.Log(new StringBuilder("Loading use: ").Append(Time.realtimeSinceStartup - stime));

            System.GC.Collect(2);

            yield return AO;
        }
    }

}