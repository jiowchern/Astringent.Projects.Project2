﻿using Astringent.Game20220410.Scripts;
using Unity.Entities;
using Unity.Jobs;
using System.Linq;
using Unity.Transforms;


namespace Astringent.Game20220410.Dots.Systems
{

    
    public partial class MoveingSystem : Unity.Entities.SystemBase 
    {

        protected override void OnCreate()
        {
            
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
         
            base.OnDestroy();
        }
        public MoveingSystem()
        {
         
        }
        
        protected override void OnUpdate()
        {
            var nowTime = Dots.Systems.Service.GetWorld().Time.ElapsedTime;
            var deltaTime = Dots.Systems.Service.GetWorld().Time.DeltaTime;
    
            var attributes = GetComponentDataFromEntity<Attributes>();

       /*     Dependency = Entities.ForEach((ref Translation tran, ref Direction dir, in DynamicBuffer<CollisionEventBufferElement> eles, in MoveingState move_state) =>
            {
                foreach (var ele in eles)
                {

                    //if (ele.State == PhysicsEventState.Exit)
                    //continue;

                    if (!attributes.HasComponent(ele.Entity))
                    {
                        continue;
                    }


                    Attributes com;
                    if (!attributes.TryGetComponent(ele.Entity, out com))
                        continue;

                    if (com.Data.Appertance != Protocol.APPEARANCE.Barrier)
                        continue;

                    UnityEngine.Debug.Log("set dir 0");
                    tran.Value -= move_state.Data.Vector * (deltaTime +0.2f);
                    dir = new Direction() { Value = Unity.Mathematics.float3.zero };

                }
            }).Schedule(Dependency);*/

            

          


            Dependency = Entities.ForEach((
                ref Past past,
                ref Unity.Physics.PhysicsVelocity velocity,
                ref Dots.MoveingState move_state,
                in Direction dir,
                in Translation translation) =>
            {
                if (Sources.Unsafe.Equal(past.Direction, dir))
                    return;
                past.Direction = dir;
                UnityEngine.Debug.Log("change dir");

                move_state.Data.StartTime = nowTime;
                move_state.Data.Position = translation.Value;
                move_state.Data.Vector = dir.Value * move_state.Speed;
                velocity.Linear = move_state.Data.Vector;

                past.Direction = dir;

            }).ScheduleParallel(Dependency);


            /*this.Dependency = Entities.ForEach((
                          ref Past past,
                          ref Direction dir,
                          in Unity.Physics.PhysicsVelocity velocity
                          ) =>
            {
                if (Unity.Mathematics.math.any(velocity.Linear != Unity.Mathematics.float3.zero))
                {
                    return;
                }

                if (Unity.Mathematics.math.any(dir.Value != Unity.Mathematics.float3.zero))
                {
                    dir.Value = Unity.Mathematics.float3.zero;
                }


            }).ScheduleParallel(Dependency);*/

            this.Dependency = Entities.ForEach((
                          ref Past past,
                          ref Direction dir,
                          in Unity.Physics.PhysicsVelocity velocity
                          ) =>
            {

                
                var d1 = Unity.Mathematics.math.normalizesafe(velocity.Linear);
                var d2 = Unity.Mathematics.math.normalizesafe(dir.Value);
                

                
                
                if (Unity.Mathematics.math.all(d1 == d2))
                {
                    return;
                }

                
                var dr = Unity.Mathematics.math.abs(d1 - d2);

                if (Unity.Mathematics.math.all(dr < new Unity.Mathematics.float3(0.01f)))
                {
                    return;
                }

                dir.Value = 0;
                UnityEngine.Debug.Log("reset dir");

            }).ScheduleParallel(Dependency);



        }
    }
}
