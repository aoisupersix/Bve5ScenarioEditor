using System;
using System.Linq;
using System.Collections.Generic;


namespace Bve5ScenarioEditor
{
    /// <summary>
    /// シナリオデータを管理するクラス
    /// </summary>
    class ScenarioDataManagement
    {
        /// <summary>
        /// シナリオデータ
        /// </summary>
        List<List<Scenario>> _snapShot;

        /// <summary>
        /// スナップショットの最新位置を示すインデックス
        /// </summary>
        public int TopIndex { get; private set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public ScenarioDataManagement()
        {
            _snapShot = new List<List<Scenario>>();
            TopIndex = -1;
        }

        /// <summary>
        /// 引数に指定されたインデックスのスナップショットのコピーを返します。
        /// </summary>
        /// <param name="index">取得するスナップショットのインデックス</param>
        /// <returns>スナップショットのコピー</returns>
        public List<Scenario> GetSnapShot(int index)
        {
            if (index >= _snapShot.Count)
                throw new IndexOutOfRangeException();

            List<Scenario> returnData = new List<Scenario>();
            foreach(var scenarios in _snapShot[index])
            {
                returnData.Add(scenarios.Copy());
            }
            TopIndex = index;
            return returnData;
        }

        /// <summary>
        /// 最新のスナップショットのコピーを返します。
        /// </summary>
        /// <returns>最新のスナップショットのコピー</returns>
        public List<Scenario> GetNewestSnapShot()
        {
            return GetSnapShot(TopIndex);
        }

        /// <summary>
        /// 最初のスナップショットのコピーを返します。
        /// </summary>
        /// <returns></returns>
        public List<Scenario> GetOldestSnapShot()
        {
            return GetSnapShot(0);
        }

        /// <summary>
        /// 最新のシナリオデータを新たに追加します。
        /// </summary>
        /// <param name="snap">追加するシナリオデータ</param>
        public void SetNewMemento(List<Scenario> snap)
        {
            //スナップショットをDeepCopy
            List<Scenario> copy = new List<Scenario>();
            foreach (var scenarios in snap)
            {
                copy.Add(scenarios.Copy());
            }

            //最新より先にあるスナップショットを削除
            if(TopIndex > -1)
            {
                _snapShot.RemoveRange(TopIndex, _snapShot.Count - (TopIndex + 1));
            }
            _snapShot.Add(copy);
            TopIndex++;
        }

        /// <summary>
        /// 最新のシナリオデータに編集されたデータがいくつ含まれているかを取得します。
        /// </summary>
        /// <returns>編集されているデータの数</returns>
        public int NewestSnapEditCount()
        {
            return _snapShot[TopIndex].Count(x => x.DidEdit);
        }
    }
}
