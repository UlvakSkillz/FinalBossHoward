using MelonLoader;
using UnityEngine;
using static RUMBLE.Environment.Howard.HowardAttackBehaviour;

namespace FinalBossHoward
{
    class HowardMoveSet
    {
        public Vector2 RequiredMinMaxRange;
        public float Weight;
        public TimedStack[] timedStack;
        public float[] PreWaitTime, PostWaitTime;
        private int spot = 0;

        public HowardMoveSet(int moveCount)
        {
            timedStack = new TimedStack[moveCount];
            PreWaitTime = new float[moveCount];
            PostWaitTime = new float[moveCount];
        }

        public void AddMove(TimedStack stackToAdd)
        {
            timedStack[spot] = stackToAdd;
            spot++;
        }

        public void AddMove(TimedStack stackToAdd, float preWaitTime, float postWaitTime)
        {
            timedStack[spot] = stackToAdd;
            PreWaitTime[spot] = preWaitTime;
            PostWaitTime[spot] = postWaitTime;
            spot++;
        }

        public void AddMove(TimedStack stackToAdd, float preWaitTime, float postWaitTime, bool isPersistent)
        {
            timedStack[spot] = stackToAdd;
            PreWaitTime[spot] = preWaitTime;
            PostWaitTime[spot] = postWaitTime;
            timedStack[spot].IsPersistentStack = isPersistent;
            if (isPersistent)
            {
                timedStack[spot].PersistentStackWaitTime = preWaitTime + postWaitTime;
            }
            else
            {
                timedStack[spot].PersistentStackWaitTime = 0;
            }
            spot++;
        }

        public void AddMove(TimedStack stackToAdd, float preWaitTime, float postWaitTime, bool isPersistent, float persistentStackWaitTime)
        {
            timedStack[spot] = stackToAdd;
            PreWaitTime[spot] = preWaitTime;
            PostWaitTime[spot] = postWaitTime;
            timedStack[spot].IsPersistentStack = isPersistent;
            timedStack[spot].PersistentStackWaitTime = persistentStackWaitTime;
            spot++;
        }

        public void SetRangeAndWeight(Vector2 requiredMinMaxRange, float weight)
        {
            RequiredMinMaxRange = requiredMinMaxRange;
            Weight = weight;
        }
    }
}
