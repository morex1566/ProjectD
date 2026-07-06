using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class CreatureStateMachine
    {
        [SerializeField] private SerializableDictionary<CreatureStateType, CreatureState> currentStates = new();

        private CreatureController controller;


        public IReadOnlyDictionary<CreatureStateType, CreatureState> CurrentStates => currentStates.ReadOnlyDictionary;

        public CreatureStateMachine(CreatureController controller)
        {
            this.controller = controller;

            currentStates.Clear();
            currentStates.Add(CreatureStateType.Idle, new IdleState(controller));
        }


        /// <summary>
        /// 등록된 상태를 갱신
        /// </summary>
        public void Update()
        {
            foreach (CreatureState state in currentStates.Values)
            {
                state.Update();
            }
        }

        public void DrawGizmos()
        {
            foreach (CreatureState state in currentStates.Values)
            {
                if (state == null)
                {
                    continue;
                }

                state.DrawGizmos();
            }
        }
    }
}
