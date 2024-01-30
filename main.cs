using HarmonyLib;
using MelonLoader;
using RUMBLE.Environment.Howard;
using UnityEngine;

namespace FinalBossHoward
{
    [HarmonyPatch(typeof(Howard), "OnActivationLeverChanged")]
    public static class Patch
    {
        private static void Prefix(int step)
        {
            switch (step)
            {
                case (0):
                    if (main.instance.howard.currentLogicIndex == 2)
                    {
                        main.instance.howard.incomingStructureCheckDelay = 0;
                        main.instance.howard.knockoutRegenDelay = 0;
                        main.instance.howard.lookAtSpeed = 100;
                        main.instance.howard.minIncomingStructureDistance = 10;
                        main.instance.howard.incomingStructureFrameCount = 1;
                        main.instance.howard.currentHp = 40;
                    }
                    break;
                case (1):
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

        public override void OnEarlyInitializeMelon()
        {
            instance = this;
        }

        public override void OnFixedUpdate()
        {
            if (sceneChanged)
            {
                try
                {
                    if (currentScene == "Gym")
                    {
                        BuffHowardLevel2();
                        sceneChanged = false;
                    }
                }
                catch { }
            }
        }

        private void BuffHowardLevel2()
        {
            howard = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Howard root").GetComponent<Howard>();
            howard.howardAnimator.changeLevelAnimationWaitTime = 1;
            howard.LogicLevels[2].MinMaxDecisionTime = new Vector2(0, 0);
            howard.LogicLevels[2].standStillReactiontime = 0;
            howard.LogicLevels[2].DodgeBehaviour.AnglePerSecond = 90;
            howard.LogicLevels[2].maxHealth = 40;
            var temp = howard.LogicLevels[2].reactions[0];
            temp.Weight = 0;
            howard.LogicLevels[2].reactions[0] = temp;
            temp = howard.LogicLevels[2].reactions[1];
            temp.Weight = 100;
            howard.LogicLevels[2].reactions[1] = temp;
            howard.LogicLevels[2].howardHeadlightColor = new Color(0.414f, 0, 1, 1);
            howard.LogicLevels[2].howardIdleLevelColor = new Color(0.414f, 0, 1, 1);
            howard.LogicLevels[2].howardLevelColor = new Color(0.414f, 0, 1, 1);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            sceneChanged = true;
        }
    }
}
