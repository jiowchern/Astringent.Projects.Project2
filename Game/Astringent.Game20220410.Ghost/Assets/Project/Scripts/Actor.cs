﻿using Astringent.Game20220410.Protocol;
using System;
using UnityEngine;
using System.Linq;
using UniRx;

namespace Astringent.Game20220410
{
    public class Actor : MonoBehaviour
    {

        readonly UniRx.CompositeDisposable _Disposable;

        System.Action _MoveAction;
        public Actor()
        {
            _MoveAction = () => { };
            _Disposable = new UniRx.CompositeDisposable();
        }

        public void Startup(IActor actor)
        {

            _Release();

            _SetDestroy(actor);
            _SetMove(actor);

        }

        private void _SetDestroy(IActor actor)
        {
            var obs = from agent in AgentRx.GetObservable()
                      from a in agent.QueryNotifier<IActor>().UnsupplyEvent()
                      where a == actor
                      select a;

            _Disposable.Add(obs.Subscribe(_Destroy));
        }

        private void _Destroy(IActor actor)
        {
            GameObject.Destroy(gameObject);
        }

        private void _SetMove(IActor actor)
        {
            var obs = from moveState in actor.MoveingState.ChangeObservable().Repeat()
                      select moveState;

            _Disposable.Add(obs.Subscribe(_Move));
        }

        private void _Move(MoveingState state)
        {
            UnityEngine.Debug.Log($"get actor move {state.Position}");
            this.gameObject.transform.position = state.Position;
            _MoveAction = () => {
                this.gameObject.transform.Translate(state.Vector * UnityEngine.Time.deltaTime);
            };
        }

        private void _Release()
        {
            _Disposable.Clear();
        }



        private void Update()
        {
            _MoveAction();
        }

        private void OnDestroy()
        {
            _Release();
        }
    }

}