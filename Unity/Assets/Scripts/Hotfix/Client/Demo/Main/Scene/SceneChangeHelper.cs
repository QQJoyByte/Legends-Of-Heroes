﻿namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());

            root.GetComponent<ClientSenderCompnent>().Send(new C2Room_StateSyncChangeSceneFinish());

            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_StateSyncStart waitRoom2CStart = await root.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_StateSyncStart>();

            // 等待CreateMyUnit的消息
            // Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            // M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            foreach (UnitInfo unitInfo in waitRoom2CStart.Message.UnitInfo)
            {

                Unit unit = UnitFactory.Create(currentScene, unitInfo);
                unitComponent.Add(unit);
                root.RemoveComponent<AIComponent>();
            }
            
            
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}