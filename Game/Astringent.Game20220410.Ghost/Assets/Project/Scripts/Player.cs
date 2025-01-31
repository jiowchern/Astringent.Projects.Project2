﻿using UnityEngine;

using System.Linq;
using UniRx;
using Astringent.Game20220410.Protocol;
using System.Collections.Generic;
using System;
using Regulus.Remote.Ghost;


namespace Astringent.Game20220410
{
    public class Player : AgentReactiveMonoBehaviour 
    {
        
        readonly UniRx.CompositeDisposable _Disposable;
        public Player()
        {
            _Disposable = new UniRx.CompositeDisposable();
        }
        public static double WorldTime;
        


        new private void OnDestroy()
        {
            _Disposable.Clear();
            base.OnDestroy();
        }

        public void Stop()
        {
            _Disposable.Clear();    
            var obs =
                        from agent in Observer
                        from player in agent.QueryNotifier<IPlayer>().SupplyEvent()
                        from _ in player.SetDirection(new Unity.Mathematics.float3(0, 0, 0)).RemoteValue()
                        select _;


            _Disposable.Add(obs.First().Subscribe(i => UnityEngine.Debug.Log("stop")));
        }
        public void Move()
        {
            _Disposable.Clear();
            var obs =
                from agent in Observer
                from player in agent.QueryNotifier<IPlayer>().SupplyEvent().First()
                from _ in player.SetDirection(new Unity.Mathematics.float3(1, 0, 0)).RemoteValue()
                select _;

            _Disposable.Add(obs.Subscribe(_MoveDone));
        }

        public void Quit()
        {
            _Disposable.Clear();
            var obs =
                from agent in Observer
                from player in agent.QueryNotifier<IPlayer>().SupplyEvent()
                from _ in player.Quit().RemoteValue()
                select _;

            _Disposable.Add(obs.First().Subscribe(_=>UnityEngine.Debug.Log("player quit")));
        }

        private void _MoveDone(bool obj)
        {
            UnityEngine.Debug.Log("move done");
        }

        protected override IEnumerable<IDisposable> _Start(IAgent agent)
        {
            yield return Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => _Move(agent));

            yield return Observable.EveryUpdate().Subscribe(_ => WorldTime += UnityEngine.Time.deltaTime);

            var obs = from player in agent.QueryNotifier<IPlayer>().SupplyEvent()
                      from time in player.WorldTime.ChangeObservable().Repeat()
                      select time;
            yield return obs.Subscribe(_SetWorldTime);
        }

        private void _SetWorldTime(double time)
        {
           
            WorldTime = time;
            
        }

        private void _Move(IAgent agent)
        {
            var pos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(pos);

            
            RaycastHit info;
            if (Physics.Raycast(ray, out info,Mathf.Infinity , 1 << 6) == false)
                return;

            _Disposable.Clear();
            var obs = from player in agent.QueryNotifier<IPlayer>().SupplyEvent().First()
                      from actor in GameObject.FindObjectsOfType<Entity>()
                      where player.Id.Value == actor.Id
                      from _ in _Move(actor, player, info.point).RemoteValue()
                      select _;

            _Disposable.Add(obs.Subscribe(r=>UnityEngine.Debug.Log($"move done.")));
        }

        private Regulus.Remote.Value<bool> _Move(Entity actor, IPlayer player, Vector3 point)
        {
            var dir = point - actor.transform.position;
            actor.SetDirection(dir);
            return player.SetDirection(dir); 
        }
    }

}
