using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace WorldStreamer2
{
    /// <summary>
    /// World mover - movesw the world when moving out of chosen range.
    /// </summary>
    public class WorldMover : MonoBehaviour
    {
        /// <summary>
        /// The WORLD MOVER TAG.
        /// </summary>
        public static string WORLDMOVERTAG = "WorldMover";

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The x tile range based on main streamer.
        /// </summary>
        public float xTileRange = 2;

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The y tile range  based on main streamer.
        /// </summary>
        public float yTileRange = 2;

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The z tile range  based on main streamer.
        /// </summary>
        public float zTileRange = 2;

        [Tooltip("Time from reaching frequency distance for world mover to restart world position.")]
        /// <summary>
        /// Debug value used for client-server communication it's position without floating point fix and looping
        /// </summary>
        public float waitForRestart = 10;

        [HideInInspector]
        /// <summary>
        /// The x current tile move.
        /// </summary>
        public float xCurrentTile = 0;

        [HideInInspector]
        /// <summary>
        /// The y current tile move.
        /// </summary>
        public float yCurrentTile = 0;

        [HideInInspector]
        /// <summary>
        /// The z current tile move.
        /// </summary>
        public float zCurrentTile = 0;


        [Tooltip("Drag and drop here, your _Streamer_Major prefab from scene hierarchy.")]
        /// <summary>
        /// The streamer main for checking range.
        /// </summary>
        public Streamer streamerMajor;


        [Tooltip("Drag and drop here, your all _Streamer_Minors prefabs from scene hierarchy.")]
        /// <summary>
        /// The additional streamers to move tiles.
        /// </summary>
        public Streamer[] streamerMinors;

        [Tooltip("Differences between real  and restarted player position. Useful in AI and network communications.")]
        /// <summary>
        /// The current move vector.
        /// </summary>
        public Vector3 currentMove = Vector3.zero;

        /// <summary>
        /// The objects to move with tiles.
        /// </summary>
        [HideInInspector]
        public List<Transform>
            objectsToMove = new List<Transform>();

        [Tooltip("Debug value used for client-server communication it's position without floating point fix and looping")]
        /// <summary>
        /// Debug value used for client-server communication it's position without floating point fix and looping
        /// </summary>
        public Vector3 playerPositionMovedLooped;



        private Vector3 worldSize;

        bool waitForMover = false;

        /// <summary>
        /// Start this instance and sets main streamer field for world mover.
        /// </summary>
        public void Start()
        {
            streamerMajor.worldMover = this;
            List<Streamer> streamersTemp = new List<Streamer>();
            streamersTemp.AddRange(streamerMinors);
            streamersTemp.Remove(streamerMajor);
            streamerMinors = streamersTemp.ToArray();

            worldSize = new Vector3(streamerMajor.sceneCollectionManagers[0].xSize * (streamerMajor.sceneCollectionManagers[0].xLimitsy - streamerMajor.sceneCollectionManagers[0].xLimitsx + 1),
                streamerMajor.sceneCollectionManagers[0].ySize * (streamerMajor.sceneCollectionManagers[0].yLimitsy - streamerMajor.sceneCollectionManagers[0].yLimitsx + 1),
                streamerMajor.sceneCollectionManagers[0].zSize * (streamerMajor.sceneCollectionManagers[0].zLimitsy - streamerMajor.sceneCollectionManagers[0].zLimitsx + 1));


            Debug.Log("World Mover worldSize - " + worldSize);
        }

        public void Update()
        {
            if (streamerMajor.player != null)
            {
                playerPositionMovedLooped = streamerMajor.player.position - currentMove;

                if (streamerMajor.looping)
                {

                    //Debug.Log (playerPositionMovedLooped.z + " " + Mathf.Abs (streamerMajor.sceneCollection.zSize * streamerMajor.sceneCollection.zLimitsx) + " " + worldSize.z);

                    playerPositionMovedLooped = new Vector3(worldSize.x != 0 ? modf((playerPositionMovedLooped.x + Mathf.Abs(streamerMajor.sceneCollectionManagers[0].xSize * streamerMajor.sceneCollectionManagers[0].xLimitsx)), worldSize.x) + streamerMajor.sceneCollectionManagers[0].xSize * streamerMajor.sceneCollectionManagers[0].xLimitsx : playerPositionMovedLooped.x,
                        worldSize.y != 0 ? modf((playerPositionMovedLooped.y + Mathf.Abs(streamerMajor.sceneCollectionManagers[0].ySize * streamerMajor.sceneCollectionManagers[0].yLimitsx)), worldSize.y) + streamerMajor.sceneCollectionManagers[0].ySize * streamerMajor.sceneCollectionManagers[0].yLimitsx : playerPositionMovedLooped.y,
                        worldSize.z != 0 ? modf((playerPositionMovedLooped.z + Mathf.Abs(streamerMajor.sceneCollectionManagers[0].zSize * streamerMajor.sceneCollectionManagers[0].zLimitsx)), worldSize.z) + streamerMajor.sceneCollectionManagers[0].zSize * streamerMajor.sceneCollectionManagers[0].zLimitsx : playerPositionMovedLooped.z);


                }

            }


        }

        /// <summary>
        /// Checks the mover distance.
        /// </summary>
        /// <param name="xPosCurrent">X position current in tiles.</param>
        /// <param name="yPosCurrent">Y position current in tiles.</param>
        /// <param name="zPosCurrent">Z position current in tiles.</param>
        public void CheckMoverDistance(int xPosCurrent, int yPosCurrent, int zPosCurrent)
        {
            if (waitForMover)
                return;

            if (Mathf.Abs(xPosCurrent - xCurrentTile) > xTileRange || Mathf.Abs(yPosCurrent - yCurrentTile) > yTileRange || Mathf.Abs(zPosCurrent - zCurrentTile) > zTileRange)
            {
                waitForMover = true;
                StartCoroutine(MoveWorld(xPosCurrent, yPosCurrent, zPosCurrent));

            }
        }

        /// <summary>
        /// Moves the world.
        /// </summary>
        /// <param name="xPosCurrent">X position current in tiles.</param>
        /// <param name="yPosCurrent">Y position current in tiles.</param>
        /// <param name="zPosCurrent">Z position current in tiles.</param>
        IEnumerator MoveWorld(int xPosCurrent, int yPosCurrent, int zPosCurrent)
        {
            //Debug.Log("PreMoveWorld " + xPosCurrent + " " + yPosCurrent + " " + zPosCurrent);
            yield return new WaitForSeconds(waitForRestart);

            //Debug.Log("MoveWorld " + xPosCurrent + " " + yPosCurrent + " " + zPosCurrent);

            Vector3 moveVector = new Vector3((xPosCurrent - xCurrentTile) * streamerMajor.sceneCollectionManagers[0].xSize, (yPosCurrent - yCurrentTile) * streamerMajor.sceneCollectionManagers[0].ySize, (zPosCurrent - zCurrentTile) * streamerMajor.sceneCollectionManagers[0].zSize);

            currentMove -= moveVector;

            streamerMajor.player.position -= moveVector;
            foreach (var scm in streamerMajor.sceneCollectionManagers)
                foreach (var item in scm.loadedScenes)
                {
                    if (item.loaded && item.sceneGo != null)
                        item.sceneGo.transform.position -= moveVector;
                }
            Vector3 position;
            foreach (var item in objectsToMove)
            {

                if (item != null)
                {
                    //Debug.Log (item.name);
                    position = item.position;
                    position -= moveVector;

                    //if (position.x > worldSize.x)
                    //    position.x -= worldSize.x;
                    //if (position.x < 0)
                    //    position.x += worldSize.x;

                    //if (position.y > worldSize.y)
                    //    position.y -= worldSize.y;
                    //if (position.y < 0)
                    //    position.y += worldSize.y;

                    //if (position.z > worldSize.z)
                    //    position.z -= worldSize.z;
                    //if (position.x < 0)
                    //    position.z += worldSize.z;

                    item.position = position;
                }
            }

            xCurrentTile = xPosCurrent;
            yCurrentTile = yPosCurrent;
            zCurrentTile = zPosCurrent;

            streamerMajor.currentMove = currentMove;

            foreach (var item in streamerMinors)
            {

                item.currentMove = currentMove;

                foreach (var scm in item.sceneCollectionManagers)
                    foreach (var scene in scm.loadedScenes)
                    {
                        if (scene.loaded && scene.sceneGo != null)
                            scene.sceneGo.transform.position -= moveVector;
                    }
            }
            waitForMover = false;
        }

        /// <summary>
        /// Moves the object.
        /// </summary>
        /// <param name="objectTransform">Object transform.</param>
        public void MoveObject(Transform objectTransform)
        {
            objectTransform.position += currentMove;
        }

        /// <summary>
        /// Adds the object to move.
        /// </summary>
        /// <param name="objectToMove">Object to move.</param>
        public void AddObjectToMove(Transform objectToMove)
        {
            transform.position += currentMove;
            objectsToMove.Add(objectToMove);
        }

        float modf(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}