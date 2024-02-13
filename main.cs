using HarmonyLib;
using MelonLoader;
using RUMBLE.Environment.Howard;
using RUMBLE.Managers;
using RUMBLE.MoveSystem;
using System;
using UnityEngine;

namespace FinalBossHoward
{
    [HarmonyPatch(typeof(Howard), "OnActivationLeverChanged")]
    public static class Patch
    {
        private static void Prefix(int step)
        {
            //step is on/off
            switch (step)
            {
                case (0):
                    //if turned on
                    if (main.instance.howard.currentLogicIndex == 2)
                    {
                        main.instance.howard.incomingStructureCheckDelay = 0;
                        main.instance.howard.knockoutRegenDelay = 0;
                        main.instance.howard.lookAtSpeed = 100;
                        main.instance.howard.minIncomingStructureDistance = 10;
                        main.instance.howard.incomingStructureFrameCount = 1;
                        main.instance.howard.currentHp = 40;
                        main.instance.checkStructuresTimer = DateTime.Now;
                        main.instance.howardActive = true;
                    }
                    break;
                case (1):
                    //if turned off
                    main.instance.howard.incomingStructureCheckDelay = 0.5f;
                    main.instance.howard.knockoutRegenDelay = 3;
                    main.instance.howard.lookAtSpeed = 30;
                    main.instance.howard.minIncomingStructureDistance = 3;
                    main.instance.howard.incomingStructureFrameCount = 7;
                    main.instance.howardActive = false;
                    break;
            }
        }
    }
    public class main : MelonMod
    {
        public static main instance;
        public Howard howard;
        private string currentScene = "";
        private bool sceneChanged = false;
        public bool howardActive = false;
        private PoolManager poolManager;
        private StackManager stackManager;
        private HowardAttackBehaviour[] howardAttackBehaviour;
        private HowardLogic.SequenceSet[] newSequenceSet;
        private GameObject discPool, ballPool, pillarPool, cubePool, wallPool, smallRockPool, largeRockPool;
        private DateTime[] discPoolTimer, ballPoolTimer, pillarPoolTimer, cubePoolTimer, wallPoolTimer, smallRockPoolTimer, largeRockPoolTimer;
        private bool[] discPoolTimerActive;
        private bool[] ballPoolTimerActive;
        private bool[] pillarPoolTimerActive;
        private bool[] cubePoolTimerActive;
        private bool[] wallPoolTimerActive;
        private bool[] smallRockPoolTimerActive;
        private bool[] largeRockPoolTimerActive;
        public DateTime checkStructuresTimer;

        public override void OnEarlyInitializeMelon()
        {
            instance = this;
        }

        public override void OnFixedUpdate()
        {
            //if scene changed
            if (sceneChanged)
            {
                try
                {
                    //if in the Gym
                    if (currentScene == "Gym")
                    {
                        //setup
                        poolManager = GameObject.Find("Game Instance/Pre-Initializable/PoolManager").GetComponent<PoolManager>();
                        stackManager = GameObject.Find("Game Instance/Initializable/StackManager").GetComponent<StackManager>();
                        SetupPoolArrays();
                        BuffHowardLevel2();
                        sceneChanged = false;
                        howard.SetCurrentLogicLevel(2);//////////////////////////////////////////////////////////////////////////////////////////////////////////
                        howard.SetHowardLogicActive(true);///////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                }
                catch (Exception e)
                {
                    //MelonLogger.Error(e);
                    return;
                }
            }
            //other scenes dont matter
            if (currentScene != "Gym")
            {
                return;
            }
            //if fighting howard
            if (howardActive)
            {
                //is structure check timer has elapsed
                if (checkStructuresTimer <= DateTime.Now)
                {
                    //update structure timers
                    ClearRails();
                    checkStructuresTimer = DateTime.Now.AddSeconds(0.75);
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            sceneChanged = true;
        }

        //sets up Structure Pools and Timers
        private void SetupPoolArrays()
        {
            //gets Structure Pools
            discPool = poolManager.transform.GetChild(43).gameObject;
            pillarPool = poolManager.transform.GetChild(42).gameObject;
            ballPool = poolManager.transform.GetChild(51).gameObject;
            cubePool = poolManager.transform.GetChild(50).gameObject;
            wallPool = poolManager.transform.GetChild(49).gameObject;
            smallRockPool = poolManager.transform.GetChild(16).gameObject;
            largeRockPool = poolManager.transform.GetChild(17).gameObject;
            //sets up Structure Timers
            discPoolTimer = new DateTime[discPool.transform.childCount];
            pillarPoolTimer = new DateTime[pillarPool.transform.childCount];
            ballPoolTimer = new DateTime[ballPool.transform.childCount];
            cubePoolTimer = new DateTime[cubePool.transform.childCount];
            wallPoolTimer = new DateTime[wallPool.transform.childCount];
            smallRockPoolTimer = new DateTime[smallRockPool.transform.childCount];
            largeRockPoolTimer = new DateTime[largeRockPool.transform.childCount];
            discPoolTimerActive = new bool[discPool.transform.childCount];
            pillarPoolTimerActive = new bool[pillarPool.transform.childCount];
            ballPoolTimerActive = new bool[ballPool.transform.childCount];
            cubePoolTimerActive = new bool[cubePool.transform.childCount];
            wallPoolTimerActive = new bool[wallPool.transform.childCount];
            smallRockPoolTimerActive = new bool[smallRockPool.transform.childCount];
            largeRockPoolTimerActive = new bool[largeRockPool.transform.childCount];
        }

        //increases arrays if needed then checks objects to see if nearby and checks timers
        private void ClearRails()
        {
            //check if arrays need to increase
            if (discPool.transform.childCount > discPoolTimerActive.Length)
            {
                IncreaseArraySizes("Disc");
            }
            if (pillarPool.transform.childCount > pillarPoolTimerActive.Length)
            {
                IncreaseArraySizes("Pillar");
            }
            if (ballPool.transform.childCount > ballPoolTimerActive.Length)
            {
                IncreaseArraySizes("Ball");
            }
            if (cubePool.transform.childCount > cubePoolTimerActive.Length)
            {
                IncreaseArraySizes("Cube");
            }
            if (wallPool.transform.childCount > wallPoolTimerActive.Length)
            {
                IncreaseArraySizes("Wall");
            }
            if (smallRockPool.transform.childCount > smallRockPoolTimerActive.Length)
            {
                IncreaseArraySizes("SmallRock");
            }
            if (largeRockPool.transform.childCount > largeRockPoolTimerActive.Length)
            {
                IncreaseArraySizes("LargeRock");
            }
            //checks pools for objects near rails
            CheckPools(discPool, "Disc");
            CheckPools(pillarPool, "Pillar");
            CheckPools(ballPool, "Ball");
            CheckPools(cubePool, "Cube");
            CheckPools(wallPool, "Wall");
            CheckPools(smallRockPool, "SmallRock");
            CheckPools(largeRockPool, "LargeRock");
            //checks timers
            CheckTimersIfDone();
        }

        //checks if structure timers to destroy are done
        private void CheckTimersIfDone()
        {
            CheckPoolTimers(discPool, discPoolTimer, discPoolTimerActive);
            CheckPoolTimers(pillarPool, pillarPoolTimer, pillarPoolTimerActive);
            CheckPoolTimers(ballPool, ballPoolTimer, ballPoolTimerActive);
            CheckPoolTimers(cubePool, cubePoolTimer, cubePoolTimerActive);
            CheckPoolTimers(wallPool, wallPoolTimer, wallPoolTimerActive);
            CheckPoolTimers(smallRockPool, smallRockPoolTimer, smallRockPoolTimerActive);
            CheckPoolTimers(largeRockPool, largeRockPoolTimer, largeRockPoolTimerActive);
        }

        //checks the given pools timers
        private void CheckPoolTimers(GameObject pool, DateTime[] poolTimer, bool[] poolTimerActive)
        {
            //for each pool timer
            for (int i = 0; i < poolTimer.Length; i++)
            {
                //if timer is active
                if (poolTimerActive[i])
                {
                    //if timer elapsed
                    if (poolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((pool.transform.GetChild(i).gameObject.active) && (pool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            pool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        poolTimerActive[i] = false;
                    }
                }
            }
        }

        //Increases the Rail Structure Destroying Arrays
        private void IncreaseArraySizes(string poolName)
        {
            //find correct pool and increase array
            switch (poolName)
            {
                case "Disc":
                    IncreasePoolArray(discPool, discPoolTimer, discPoolTimerActive);
                    break;
                case "Pillar":
                    IncreasePoolArray(pillarPool, pillarPoolTimer, pillarPoolTimerActive);
                    break;
                case "Ball":
                    IncreasePoolArray(ballPool, ballPoolTimer, ballPoolTimerActive);
                    break;
                case "Cube":
                    IncreasePoolArray(cubePool, cubePoolTimer, cubePoolTimerActive);
                    break;
                case "Wall":
                    IncreasePoolArray(wallPool, wallPoolTimer, wallPoolTimerActive);
                    break;
                case "SmallRock":
                    IncreasePoolArray(smallRockPool, smallRockPoolTimer, smallRockPoolTimerActive);
                    break;
                case "LargeRock":
                    IncreasePoolArray(largeRockPool, largeRockPoolTimer, largeRockPoolTimerActive);
                    break;
            }
        }

        private void IncreasePoolArray(GameObject pool, DateTime[] poolTimer, bool[] poolTimerActive)
        {
            DateTime[] tempDateTimeArray;
            bool[] tempBoolArray;
            tempDateTimeArray = new DateTime[pool.transform.childCount];
            tempBoolArray = new bool[pool.transform.childCount];
            for (int i = 0; i < poolTimer.Length; i++)
            {
                tempDateTimeArray[i] = poolTimer[i];
                tempBoolArray[i] = poolTimerActive[i];
            }
            poolTimer = tempDateTimeArray;
            poolTimerActive = tempBoolArray;
        }

        //checks timers to see if objects near rails need to be destroyed
        private void CheckPools(GameObject pool, string poolName)
        {
            //for every child
            for (int i = 0; i < pool.transform.GetChildCount(); i++)
            {
                //if active and at rest
                if ((pool.transform.GetChild(i).gameObject.active) && (pool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                {
                    //if too close to rails
                    if (CheckIfCloseToRails(pool.transform.GetChild(i)))
                    {
                        //set timer to active for that structure
                        switch (poolName)
                        {
                            case "Disc":
                                if (!discPoolTimerActive[i])
                                {
                                    discPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    discPoolTimerActive[i] = true;
                                }
                                break;
                            case "Pillar":
                                if (!pillarPoolTimerActive[i])
                                {
                                    pillarPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    pillarPoolTimerActive[i] = true;
                                }
                                break;
                            case "Ball":
                                if (!ballPoolTimerActive[i])
                                {
                                    ballPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    ballPoolTimerActive[i] = true;
                                }
                                break;
                            case "Cube":
                                if (!cubePoolTimerActive[i])
                                {
                                    cubePoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    cubePoolTimerActive[i] = true;
                                }
                                break;
                            case "Wall":
                                if (!wallPoolTimerActive[i])
                                {
                                    wallPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    wallPoolTimerActive[i] = true;
                                }
                                break;
                            case "SmallRock":
                                if (!smallRockPoolTimerActive[i])
                                {
                                    smallRockPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    smallRockPoolTimerActive[i] = true;
                                }
                                break;
                            case "LargeRock":
                                if (!largeRockPoolTimerActive[i])
                                {
                                    largeRockPoolTimer[i] = DateTime.Now.AddSeconds(0.75);
                                    largeRockPoolTimerActive[i] = true;
                                }
                                break;
                        }
                    }
                }
            }
        }

        
        private void SetupNewMoveset()
        {
            newSequenceSet = new HowardLogic.SequenceSet[6];
            newSequenceSet[0] = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[3]; // dodge
            newSequenceSet[1] = new HowardLogic.SequenceSet();
            newSequenceSet[2] = new HowardLogic.SequenceSet();
            newSequenceSet[3] = new HowardLogic.SequenceSet();
            newSequenceSet[4] = new HowardLogic.SequenceSet();
            newSequenceSet[5] = new HowardLogic.SequenceSet();
            SetupMoveset(newSequenceSet[1], "DiscStraight", 0, 100, 50, 25);//disc
            SetupMoveset(newSequenceSet[2], "BallUppercutStraight", 0, Vector2.positiveInfinity.y, 50, 25);//ball
            SetupMoveset(newSequenceSet[3], "PillarPillarStraightStraight", 0, 6, 50, 25);//pillar
            SetupMoveset(newSequenceSet[4], "CubeStraightUppercut", 2, Vector2.positiveInfinity.y, 50, 25);//cube
            SetupMoveset(newSequenceSet[5], "WallStraightUppercutKick", 5, Vector2.positiveInfinity.y, 50, 25);//wall
        }

        private void SetupMoveset(HowardLogic.SequenceSet sequenceSet, string name, float minRange, float maxRange, float weight, float weightDecrementationWhenSelected)
        {
            sequenceSet.currentWeightDecrementation = 0;
            sequenceSet.RequiredMinMaxRange = new Vector2(minRange, maxRange);
            sequenceSet.Sequence = new HowardSequence();
            sequenceSet.Sequence.name = name;
            sequenceSet.Sequence.BehaviourTimings = new HowardSequence.HowardBehaviourTiming[1];
            sequenceSet.Sequence.BehaviourTimings[0] = new HowardSequence.HowardBehaviourTiming();
            sequenceSet.Sequence.BehaviourTimings[0].Behaviour = new HowardAttackBehaviour();
            sequenceSet.Sequence.BehaviourTimings[0].PreActivationWaitTime = 0;
            sequenceSet.Sequence.BehaviourTimings[0].PostActivationWaitTime = 0;
            sequenceSet.Weight = weight;
            sequenceSet.WeightDecrementationWhenSelected = weightDecrementationWhenSelected;
        }

        private void CreateNewMoveset()
        {
            howardAttackBehaviour = new HowardAttackBehaviour[6];
            howardAttackBehaviour[0] = new HowardAttackBehaviour();
            howardAttackBehaviour[1] = new HowardAttackBehaviour();
            howardAttackBehaviour[2] = new HowardAttackBehaviour();
            howardAttackBehaviour[3] = new HowardAttackBehaviour();
            howardAttackBehaviour[4] = new HowardAttackBehaviour();
            howardAttackBehaviour[5] = new HowardAttackBehaviour();
            for (int i = 1; i < newSequenceSet.Length; i++)
            {
                howardAttackBehaviour[i - 1] = newSequenceSet[i].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>();
            }
            //Disc Straight
            howardAttackBehaviour[1].timedStacks = new HowardAttackBehaviour.TimedStack[2];
            for(int i = 0; i < howardAttackBehaviour[1].timedStacks.Length; i++)
            {
                howardAttackBehaviour[1].timedStacks[i] = new HowardAttackBehaviour.TimedStack();
            }
            SetStack(howardAttackBehaviour[1].timedStacks[0], 0, "SpawnStructure", true, 0, 0, 0.1f, stackManager.allStacks[0]);
            SetStack(howardAttackBehaviour[1].timedStacks[1], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[25]);
            //Ball Uppercut Straight
            howardAttackBehaviour[2].timedStacks = new HowardAttackBehaviour.TimedStack[3];
            for (int i = 0; i < howardAttackBehaviour[2].timedStacks.Length; i++)
            {
                howardAttackBehaviour[2].timedStacks[i] = new HowardAttackBehaviour.TimedStack();
            }
            SetStack(howardAttackBehaviour[2].timedStacks[0], 0, "SpawnStructure", false, 0, 0, 0.8f, stackManager.allStacks[20]);
            SetStack(howardAttackBehaviour[2].timedStacks[1], 0, "Straight", true, 0.1f, 0, 0.1f, stackManager.allStacks[26]);
            SetStack(howardAttackBehaviour[2].timedStacks[2], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[25]);
            //Pillar Pillar Straight Straight
            howardAttackBehaviour[3].timedStacks = new HowardAttackBehaviour.TimedStack[4];
            for (int i = 0; i < howardAttackBehaviour[3].timedStacks.Length; i++)
            {
                howardAttackBehaviour[3].timedStacks[i] = new HowardAttackBehaviour.TimedStack();
            }
            SetStack(howardAttackBehaviour[3].timedStacks[0], 0, "SpawnStructure", false, 0, 0, 0.4f, stackManager.allStacks[20]);
            SetStack(howardAttackBehaviour[3].timedStacks[1], 0, "SpawnStructure", false, 0, 0, 0.2f, stackManager.allStacks[20]);
            SetStack(howardAttackBehaviour[3].timedStacks[2], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[25]);
            SetStack(howardAttackBehaviour[3].timedStacks[3], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[25]);
            //Cube Straight Uppercut
            howardAttackBehaviour[4].timedStacks = new HowardAttackBehaviour.TimedStack[3];
            for (int i = 0; i < howardAttackBehaviour[4].timedStacks.Length; i++)
            {
                howardAttackBehaviour[4].timedStacks[i] = new HowardAttackBehaviour.TimedStack();
            }
            SetStack(howardAttackBehaviour[4].timedStacks[0], 0, "SpawnStructure", false, 0, 0, 0.4f, stackManager.allStacks[21]);
            SetStack(howardAttackBehaviour[4].timedStacks[1], 0, "Straight", true, 0.1f, 0, 0.1f, stackManager.allStacks[25]);
            SetStack(howardAttackBehaviour[4].timedStacks[2], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[26]);
            //Wall Straight Uppercut Kick
            howardAttackBehaviour[5].timedStacks = new HowardAttackBehaviour.TimedStack[4];
            for (int i = 0; i < howardAttackBehaviour[5].timedStacks.Length; i++)
            {
                howardAttackBehaviour[5].timedStacks[i] = new HowardAttackBehaviour.TimedStack();
            }
            SetStack(howardAttackBehaviour[5].timedStacks[0], 0, "SpawnStructure", false, 0, 0, 0.4f, stackManager.allStacks[23]);
            SetStack(howardAttackBehaviour[5].timedStacks[1], 0, "Straight", true, 0.1f, 0, 0.1f, stackManager.allStacks[25]);
            SetStack(howardAttackBehaviour[5].timedStacks[2], 0, "Straight", true, 0.1f, 0, 0.1f, stackManager.allStacks[26]);
            SetStack(howardAttackBehaviour[5].timedStacks[3], 0, "Straight", true, 0.3f, 0, 0.3f, stackManager.allStacks[8]);
        }

        private void SetStack(HowardAttackBehaviour.TimedStack timedStack, float animationTriggerDelay, string animationTriggerName, bool isPersistentStack, float persistentStackWaitTime, float preWaitTime, float postWaitTime, Stack stack)
        {
            timedStack.AnimationTriggerDelay = animationTriggerDelay;
            timedStack.AnimationTriggerName = animationTriggerName;
            timedStack.IsPersistentStack = isPersistentStack;
            timedStack.PersistentStackWaitTime = persistentStackWaitTime;
            timedStack.PreWaitTime = preWaitTime;
            timedStack.PostWaitTime = postWaitTime;
            timedStack.Stack = stack;
        }

        //checks if structure is near the rails
        private bool CheckIfCloseToRails(Transform structure)
        {
            //left side
            if (Vector3.Distance(structure.transform.position, new Vector3(13.5198f, -2.9653f, -19.7457f)) < 4)
            {
                return true;
            }
            //right side
            if (Vector3.Distance(structure.transform.position, new Vector3(6.7449f, -2.5501f, -25.282f)) < 4)
            {
                return true;
            }
            //center
            if (Vector3.Distance(structure.transform.position, new Vector3(11.7203f, -2.9645f, -22.255f)) < 4)
            {
                return true;
            }
            return false;
        }

        //changes Howards stats and moves on scene load
        private void BuffHowardLevel2()
        {
            howard = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Howard root").GetComponent<Howard>();
            //buff stats
            howard.howardAnimator.changeLevelAnimationWaitTime = 1;
            howard.LogicLevels[2].MinMaxDecisionTime = new Vector2(0, 0);
            howard.LogicLevels[2].standStillReactiontime = 0;
            //set dodge speed
            howard.LogicLevels[2].DodgeBehaviour.AnglePerSecond = 40;
            howard.LogicLevels[2].maxHealth = 40;
            //set dodge chance
            var temp = howard.LogicLevels[2].reactions[0];
            temp.Weight = 10;
            howard.LogicLevels[2].reactions[0] = temp;
            temp = howard.LogicLevels[2].reactions[1];
            temp.Weight = 90;
            howard.LogicLevels[2].reactions[1] = temp;
            //set howards new color
            howard.LogicLevels[2].howardHeadlightColor = new Color(0.414f, 0, 1, 1);
            howard.LogicLevels[2].howardIdleLevelColor = new Color(0.414f, 0, 1, 1);
            howard.LogicLevels[2].howardLevelColor = new Color(0.414f, 0, 1, 1);
            SetupNewMoveset();
            CreateNewMoveset();
            for (int i = 0; i < newSequenceSet.Length; i++)
            {
                Howard.currentActiveHoward.LogicLevels[2].SequenceSets[i] = newSequenceSet[i];
            }
            GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>().localPlayer.Controller.transform.position = new Vector3(10f, 0, -21f);
            MelonLogger.Msg("Done");
        }
    }
}
