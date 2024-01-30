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
                        SetupPoolArrays();
                        BuffHowardLevel2();
                        sceneChanged = false;
                    }
                }
                catch
                {
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
                    checkStructuresTimer = DateTime.Now.AddSeconds(1);
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
            //for each disc timer
            for (int i = 0; i < discPoolTimer.Length; i++)
            {
                //if timer is active
                if (discPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (discPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((discPool.transform.GetChild(i).gameObject.active) && (discPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            discPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        discPoolTimerActive[i] = false;
                    }
                }
            }
            //for each pillar timer
            for (int i = 0; i < pillarPoolTimer.Length; i++)
            {
                //if timer is active
                if (pillarPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (pillarPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((pillarPool.transform.GetChild(i).gameObject.active) && (pillarPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            pillarPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        pillarPoolTimerActive[i] = false;
                    }
                }
            }
            //for each ball timer
            for (int i = 0; i < ballPoolTimer.Length; i++)
            {
                //if timer is active
                if (ballPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (ballPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((ballPool.transform.GetChild(i).gameObject.active) && (ballPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            ballPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        ballPoolTimerActive[i] = false;
                    }
                }
            }
            //for each cube timer
            for (int i = 0; i < cubePoolTimer.Length; i++)
            {
                //if timer is active
                if (cubePoolTimerActive[i])
                {
                    //if timer elapsed
                    if (cubePoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((cubePool.transform.GetChild(i).gameObject.active) && (cubePool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            cubePool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        cubePoolTimerActive[i] = false;
                    }
                }
            }
            //for each wall timer
            for (int i = 0; i < wallPoolTimer.Length; i++)
            {
                //if timer is active
                if (wallPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (wallPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((wallPool.transform.GetChild(i).gameObject.active) && (wallPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            wallPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        wallPoolTimerActive[i] = false;
                    }
                }
            }
            //for each small rock timer
            for (int i = 0; i < smallRockPoolTimer.Length; i++)
            {
                //if timer is active
                if (smallRockPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (smallRockPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((smallRockPool.transform.GetChild(i).gameObject.active) && (smallRockPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            smallRockPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        smallRockPoolTimerActive[i] = false;
                    }
                }
            }
            //for each large rock timer
            for (int i = 0; i < largeRockPoolTimer.Length; i++)
            {
                //if timer is active
                if (largeRockPoolTimerActive[i])
                {
                    //if timer elapsed
                    if (largeRockPoolTimer[i] <= DateTime.Now)
                    {
                        //if structure active and at rest still
                        if ((largeRockPool.transform.GetChild(i).gameObject.active) && (largeRockPool.transform.GetChild(i).GetComponent<Structure>().CurrentSpeed == 0))
                        {
                            //destroy structure
                            largeRockPool.transform.GetChild(i).GetComponent<Structure>().Kill(new Vector3(0, 0, 0), true, true, true);
                        }
                        //deactivate timer
                        largeRockPoolTimerActive[i] = false;
                    }
                }
            }
        }

        //Increases the Rail Structure Destroying Arrays
        private void IncreaseArraySizes(string poolName)
        {
            DateTime[] tempDateTimeArray;
            bool[] tempBoolArray;
            //find correct pool and increase array
            switch (poolName)
            {
                case "Disc":
                    tempDateTimeArray = new DateTime[discPool.transform.childCount];
                    tempBoolArray = new bool[discPool.transform.childCount];
                    for (int i = 0; i < discPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = discPoolTimer[i];
                        tempBoolArray[i] = discPoolTimerActive[i];
                    }
                    try
                    {
                        discPoolTimer = tempDateTimeArray;
                        discPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "Pillar":
                    tempDateTimeArray = new DateTime[pillarPool.transform.childCount];
                    tempBoolArray = new bool[pillarPool.transform.childCount];
                    for (int i = 0; i < pillarPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = pillarPoolTimer[i];
                        tempBoolArray[i] = pillarPoolTimerActive[i];
                    }
                    try
                    {
                        pillarPoolTimer = tempDateTimeArray;
                        pillarPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "Ball":
                    tempDateTimeArray = new DateTime[ballPool.transform.childCount];
                    tempBoolArray = new bool[ballPool.transform.childCount];
                    for (int i = 0; i < ballPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = ballPoolTimer[i];
                        tempBoolArray[i] = ballPoolTimerActive[i];
                    }
                    try
                    {
                        ballPoolTimer = tempDateTimeArray;
                        ballPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "Cube":
                    tempDateTimeArray = new DateTime[cubePool.transform.childCount];
                    tempBoolArray = new bool[cubePool.transform.childCount];
                    for (int i = 0; i < cubePoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = cubePoolTimer[i];
                        tempBoolArray[i] = cubePoolTimerActive[i];
                    }
                    try
                    {
                        cubePoolTimer = tempDateTimeArray;
                        cubePoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "Wall":
                    tempDateTimeArray = new DateTime[wallPool.transform.childCount];
                    tempBoolArray = new bool[wallPool.transform.childCount];
                    for (int i = 0; i < wallPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = wallPoolTimer[i];
                        tempBoolArray[i] = wallPoolTimerActive[i];
                    }
                    try
                    {
                        wallPoolTimer = tempDateTimeArray;
                        wallPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "SmallRock":
                    tempDateTimeArray = new DateTime[smallRockPool.transform.childCount];
                    tempBoolArray = new bool[smallRockPool.transform.childCount];
                    for (int i = 0; i < smallRockPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = smallRockPoolTimer[i];
                        tempBoolArray[i] = smallRockPoolTimerActive[i];
                    }
                    try
                    {
                        smallRockPoolTimer = tempDateTimeArray;
                        smallRockPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
                case "LargeRock":
                    tempDateTimeArray = new DateTime[largeRockPool.transform.childCount];
                    tempBoolArray = new bool[largeRockPool.transform.childCount];
                    for (int i = 0; i < largeRockPoolTimer.Length; i++)
                    {
                        tempDateTimeArray[i] = largeRockPoolTimer[i];
                        tempBoolArray[i] = largeRockPoolTimerActive[i];
                    }
                    try
                    {
                        largeRockPoolTimer = tempDateTimeArray;
                        largeRockPoolTimerActive = tempBoolArray;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    break;
            }
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
                                    discPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    discPoolTimerActive[i] = true;
                                }
                                break;
                            case "Pillar":
                                if (!pillarPoolTimerActive[i])
                                {
                                    pillarPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    pillarPoolTimerActive[i] = true;
                                }
                                break;
                            case "Ball":
                                if (!ballPoolTimerActive[i])
                                {
                                    ballPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    ballPoolTimerActive[i] = true;
                                }
                                break;
                            case "Cube":
                                if (!cubePoolTimerActive[i])
                                {
                                    cubePoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    cubePoolTimerActive[i] = true;
                                }
                                break;
                            case "Wall":
                                if (!wallPoolTimerActive[i])
                                {
                                    wallPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    wallPoolTimerActive[i] = true;
                                }
                                break;
                            case "SmallRock":
                                if (!smallRockPoolTimerActive[i])
                                {
                                    smallRockPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    smallRockPoolTimerActive[i] = true;
                                }
                                break;
                            case "LargeRock":
                                if (!largeRockPoolTimerActive[i])
                                {
                                    largeRockPoolTimer[i] = DateTime.Now.AddSeconds(1);
                                    largeRockPoolTimerActive[i] = true;
                                }
                                break;
                        }
                    }
                }
            }
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
            //buff moves
            //grab old move sets
            HowardMoveBehaviour howardMoveBehaviour = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[3].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardMoveBehaviour>(); //Passive Movement
            HowardAttackBehaviour howardAttackBehaviour1 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[0].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Ball Straight
            HowardAttackBehaviour howardAttackBehaviour2 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[1].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Ball Uppercut
            HowardAttackBehaviour howardAttackBehaviour3 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[2].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Cube Kick Straight
            HowardAttackBehaviour howardAttackBehaviour4 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[4].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Wall Straight Kick
            HowardAttackBehaviour howardAttackBehaviour5 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[5].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Disc
            HowardAttackBehaviour howardAttackBehaviour6 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[6].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Pillar Kick Straight
            //create new movesets
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets.Add(new HowardLogic.SequenceSet());
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[7].Sequence = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[6].Sequence;
            HowardAttackBehaviour howardAttackBehaviour7 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[7].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Pillar Pillar Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets.Add(new HowardLogic.SequenceSet());
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[8].Sequence = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[0].Sequence;
            HowardAttackBehaviour howardAttackBehaviour8 = Howard.currentActiveHoward.LogicLevels[2].SequenceSets[8].Sequence.BehaviourTimings[0].Behaviour.Cast<HowardAttackBehaviour>(); //Ball Ground Straight
            //Store Howards Moves
            HowardAttackBehaviour.TimedStack[] howardsMoves = new HowardAttackBehaviour.TimedStack[8];
            howardsMoves[0] = howardAttackBehaviour5.timedStacks[0]; //Disc
            howardsMoves[1] = howardAttackBehaviour6.timedStacks[0]; //Pillar
            howardsMoves[2] = howardAttackBehaviour1.timedStacks[0]; //Ball
            howardsMoves[3] = howardAttackBehaviour3.timedStacks[0]; //Cube
            howardsMoves[4] = howardAttackBehaviour4.timedStacks[0]; //Wall
            howardsMoves[5] = howardAttackBehaviour1.timedStacks[1]; //Straight
            howardsMoves[6] = howardAttackBehaviour2.timedStacks[1]; //Uppercut
            howardsMoves[7] = howardAttackBehaviour3.timedStacks[2]; //Kick
            //set passive move speed
            howardMoveBehaviour.AnglePerSecond = 30;
            //start modifing poses
            //0: Ball Straight (modified timings)
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[0].RequiredMinMaxRange = new Vector2(7, 10);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[0].Weight = 50;
            howardAttackBehaviour1.timedStacks[0] = howardsMoves[2];
            howardAttackBehaviour1.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour1.timedStacks[0].PostWaitTime = 0.4f;
            howardAttackBehaviour1.timedStacks[1] = howardsMoves[5];
            howardAttackBehaviour1.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour1.timedStacks[1].PostWaitTime = 0.3f;
            //1: Ball Uppercut (modified timings)
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[1].RequiredMinMaxRange = new Vector2(5, 7);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[1].Weight = 50;
            howardAttackBehaviour2.timedStacks[0] = howardsMoves[2];
            howardAttackBehaviour2.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour2.timedStacks[0].PostWaitTime = 0.4f;
            howardAttackBehaviour2.timedStacks[1] = howardsMoves[6];
            howardAttackBehaviour2.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour2.timedStacks[1].PostWaitTime = 0.3f;
            //2: Cube Uppercut Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[2].RequiredMinMaxRange = new Vector2(0, 10);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[2].Weight = 50;
            howardAttackBehaviour3.timedStacks[0] = howardsMoves[3];
            howardAttackBehaviour3.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour3.timedStacks[0].PostWaitTime = 0.75f;
            howardAttackBehaviour3.timedStacks[1] = howardsMoves[6];
            howardAttackBehaviour3.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour3.timedStacks[1].PostWaitTime = 0.5f;
            howardAttackBehaviour3.timedStacks[2] = howardsMoves[5];
            howardAttackBehaviour3.timedStacks[2].PreWaitTime = 0.1f;
            howardAttackBehaviour3.timedStacks[2].PostWaitTime = 0.3f;
            //4: Wall Cube Straight Uppercut Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[4].RequiredMinMaxRange = new Vector2(1, 8);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[4].Weight = 50;
            howardAttackBehaviour4.timedStacks = new HowardAttackBehaviour.TimedStack[5];
            howardAttackBehaviour4.timedStacks[0] = howardsMoves[4];
            howardAttackBehaviour4.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[0].PostWaitTime = 0.5f;
            howardAttackBehaviour4.timedStacks[1] = howardsMoves[3];
            howardAttackBehaviour4.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[1].PostWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[2] = howardsMoves[5];
            howardAttackBehaviour4.timedStacks[2].PreWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[2].PostWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[3] = howardsMoves[6];
            howardAttackBehaviour4.timedStacks[3].PreWaitTime = 0.25f;
            howardAttackBehaviour4.timedStacks[3].PostWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[4] = howardsMoves[5];
            howardAttackBehaviour4.timedStacks[4].PreWaitTime = 0.1f;
            howardAttackBehaviour4.timedStacks[4].PostWaitTime = 0.3f;
            //5: Disc Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[5].Weight = 50;
            howardAttackBehaviour5.timedStacks = new HowardAttackBehaviour.TimedStack[2];
            howardAttackBehaviour5.timedStacks[0] = howardsMoves[0];
            howardAttackBehaviour5.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour5.timedStacks[0].PostWaitTime = 0.001f;
            howardAttackBehaviour5.timedStacks[1] = howardsMoves[5];
            howardAttackBehaviour5.timedStacks[1].PreWaitTime = 0.001f;
            howardAttackBehaviour5.timedStacks[1].PostWaitTime = 0.3f;
            //6: Pillar Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[6].RequiredMinMaxRange = new Vector2(0, 6);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[6].Weight = 50;
            howardAttackBehaviour6.timedStacks = new HowardAttackBehaviour.TimedStack[2];
            howardAttackBehaviour6.timedStacks[0] = howardsMoves[1];
            howardAttackBehaviour6.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour6.timedStacks[0].PostWaitTime = 0.5f;
            howardAttackBehaviour6.timedStacks[1] = howardsMoves[5];
            howardAttackBehaviour6.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour6.timedStacks[1].PostWaitTime = 0.3f;
            //7: Pillar Pillar Straight Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[7].RequiredMinMaxRange = new Vector2(3, 6);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[7].Weight = 50;
            howardAttackBehaviour7.timedStacks = new HowardAttackBehaviour.TimedStack[4];
            howardAttackBehaviour7.timedStacks[0] = howardsMoves[1];
            howardAttackBehaviour7.timedStacks[0].PreWaitTime = 0.1f;
            howardAttackBehaviour7.timedStacks[0].PostWaitTime = 0.5f;
            howardAttackBehaviour7.timedStacks[1] = howardsMoves[1];
            howardAttackBehaviour7.timedStacks[1].PreWaitTime = 0.5f;
            howardAttackBehaviour7.timedStacks[1].PostWaitTime = 0.3f;
            howardAttackBehaviour7.timedStacks[2] = howardsMoves[5];
            howardAttackBehaviour7.timedStacks[2].PreWaitTime = 0.5f;
            howardAttackBehaviour7.timedStacks[2].PostWaitTime = 0.3f;
            howardAttackBehaviour7.timedStacks[3] = howardsMoves[5];
            howardAttackBehaviour7.timedStacks[3].PreWaitTime = 0.5f;
            howardAttackBehaviour7.timedStacks[3].PostWaitTime = 0.3f;
            //8: Ball Uppercut Straight
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[8].RequiredMinMaxRange = new Vector2(2, 7);
            Howard.currentActiveHoward.LogicLevels[2].SequenceSets[8].Weight = 50;
            howardAttackBehaviour8.timedStacks = new HowardAttackBehaviour.TimedStack[3];
            howardAttackBehaviour8.timedStacks[0] = howardsMoves[2];
            howardAttackBehaviour8.timedStacks[0].PreWaitTime = 0.5f;
            howardAttackBehaviour8.timedStacks[0].PostWaitTime = 0.75f;
            howardAttackBehaviour8.timedStacks[1] = howardsMoves[6];
            howardAttackBehaviour8.timedStacks[1].PreWaitTime = 0.1f;
            howardAttackBehaviour8.timedStacks[1].PostWaitTime = 0.05f;
            howardAttackBehaviour8.timedStacks[2] = howardsMoves[5];
            howardAttackBehaviour8.timedStacks[2].PreWaitTime = 0.05f;
            howardAttackBehaviour8.timedStacks[2].PostWaitTime = 0.3f;
        }
    }
}
