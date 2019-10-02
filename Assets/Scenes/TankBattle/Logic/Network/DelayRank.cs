using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace TankBattle
{
    public class DelayRank : NetworkBehaviour
    {
        readonly SyncListItem delayList = new SyncListItem();


        public void AddAndSort(uint netId,uint delay) {
            delayList.Add(new Item { netId = netId, delayValue = delay });
            Sort();
        } 

        public void ModifyAndSort(uint netId,uint newValue) {
            var index = delayList.FindIndex(x => x.netId == netId);
            var temp = delayList[index];
            temp.delayValue = newValue;
            delayList[index] = temp;
            Sort();
        }

        public uint GetFirstNetId() {
            return delayList[0].netId;
        }


        /// <summary>
        /// 根据延迟排序
        /// </summary>
        private void Sort() {
            for (int i = 0; i < delayList.Count; i++) {
                for (int j = i + 1; j < delayList.Count; j++) {
                    var baseValue = delayList[i];
                    var willComparedValue = delayList[j];
                    if(willComparedValue.delayValue < baseValue.delayValue) {
                        //交换
                        delayList[i] = willComparedValue;
                        delayList[j] = baseValue;
                    }
                }
            }
            //排序结束
        }


        public struct Item
        {
            public uint netId;
            public uint delayValue;
        }

        public class SyncListItem : SyncList<Item>
        {

        }


    }
}


