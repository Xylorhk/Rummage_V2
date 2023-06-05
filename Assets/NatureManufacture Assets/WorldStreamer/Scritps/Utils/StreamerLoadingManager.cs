using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldStreamer2
{
    public class StreamerLoadingManager
    {
        public Streamer streamer;

        List<Scene> scenesToUnload = new List<Scene>();
        public int ScenesToUnloadCount { get { return scenesToUnload.Count; } }
        List<SceneSplit> scenesToLoad = new List<SceneSplit>();
        public int ScenesToLoadCount { get { return scenesToLoad.Count; } }
        List<AsyncOperation> asyncOperations = new List<AsyncOperation>();
        public int AsyncOperationsCount { get { return asyncOperations.Count; } }

        LoadingState loadingState = LoadingState.Loading;


        enum LoadingState
        {
            Loading,
            Unloading
        }

        bool operationStarted = false;

        public void Update()
        {

            if (operationStarted)
                return;

            //Debug.Log(asyncOperations.Count);
            if (asyncOperations.Count > 0)
                return;

            if (loadingState == LoadingState.Unloading)
            {
                if (scenesToLoad.Count > 0)
                {
                    loadingState = LoadingState.Loading;
                }
                else if (scenesToUnload.Count > 0)
                {
                    // Debug.Log("scenesToUnload " + scenesToUnload.Count + " " + asyncOperations.Count);
                    operationStarted = true;
                    streamer.StartCoroutine(UnloadAsync());
                    //Unload();
                }

            }

            if (loadingState == LoadingState.Loading)
            {

                if (scenesToLoad.Count > 0)
                {
                    // Debug.Log("scenesToLoad " + scenesToLoad.Count + " " + asyncOperations.Count);
                    operationStarted = true;
                    //Load();
                    streamer.StartCoroutine(LoadAsync());
                }

                if (scenesToLoad.Count == 0)
                    loadingState = LoadingState.Unloading;
            }




        }

        void Load()
        {
            //yield return null;
            //yield return null;


            int[] sceneLoadIndexes = new int[scenesToLoad.Count];
            // for (int i = 0; i < scenesToLoad.Count; i++)
            // {
            int sceneID = SceneManager.sceneCount;
            SceneSplit split = scenesToLoad[0];
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(split.sceneName, LoadSceneMode.Additive);


            asyncOperation.completed += (operation) =>
            {

                SceneLoadComplete(sceneID, split);
                OnOperationDone(operation);

            };


            asyncOperations.Add(asyncOperation);
            //}

            scenesToLoad.RemoveAt(0);
            //scenesToLoad.Clear();
            // Debug.Log("Load finished " + asyncOperations.Count);
            operationStarted = false;
        }


        IEnumerator LoadAsync()
        {
            // yield return new WaitForSeconds(0.5f);

            //Debug.Log("scenesToLoad " + scenesToLoad.Count);
            int[] sceneLoadIndexes = new int[scenesToLoad.Count];
            for (int i = 0; i < scenesToLoad.Count; i++)
            {
                int sceneID = SceneManager.sceneCount;
                SceneSplit split = scenesToLoad[i];
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(split.sceneName, LoadSceneMode.Additive);


                asyncOperation.completed += (operation) =>
                {

                    SceneLoadComplete(sceneID, split);
                    OnOperationDone(operation);

                };


                asyncOperations.Add(asyncOperation);
                yield return null;
            }
            scenesToLoad.Clear();
            //Debug.Log("Load finished " + asyncOperations.Count);
            operationStarted = false;
        }


        private void SceneLoadComplete(int sceneID, SceneSplit split)
        {
            // Debug.Log(SceneManager.GetSceneAt(sceneID).name + " " + sceneName);
            streamer.StartCoroutine(SceneLoadCompleteAsync(sceneID, split));


        }

        private IEnumerator SceneLoadCompleteAsync(int sceneID, SceneSplit split)
        {
            yield return null;
            streamer.OnSceneLoaded(SceneManager.GetSceneAt(sceneID), split);
        }

        private void OnOperationDone(AsyncOperation asyncOperation)
        {
            streamer.StartCoroutine(RemoveAsyncOperation(asyncOperation));
        }

        private IEnumerator RemoveAsyncOperation(AsyncOperation asyncOperation)
        {
            yield return null;
            yield return null;
            asyncOperations.Remove(asyncOperation);
        }

        void Unload()
        {

            // Debug.Log(asyncOperations.Count);

            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scenesToUnload[0]);
            asyncOperation.completed += OnOperationDone;
            asyncOperations.Add(asyncOperation);

            // yield return null;

            scenesToUnload.RemoveAt(0);


            //Debug.Log("Unload finished " + asyncOperations.Count);
            operationStarted = false;
        }

        IEnumerator UnloadAsync()
        {
            yield return null;

            //Debug.Log(asyncOperations.Count);
            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scenesToUnload[i]);
                asyncOperation.completed += OnOperationDone;
                asyncOperations.Add(asyncOperation);

                yield return null;
            }
            scenesToUnload.Clear();


            //Debug.Log("Unload finished " + asyncOperations.Count);
            operationStarted = false;
        }

        public void UnloadSceneAsync(Scene scene)
        {
            scenesToUnload.Add(scene);
        }

        public void LoadSceneAsync(SceneSplit split)
        {
            scenesToLoad.Add(split);
        }
    }

}