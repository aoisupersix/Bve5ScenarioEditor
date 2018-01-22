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
        public Stack<List<Scenario>> SnapShot { get; private set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public ScenarioDataManagement()
        {
            SnapShot = new Stack<List<Scenario>>();
        }

        /// <summary>
        /// 最新のシナリオデータを新たに追加します。
        /// </summary>
        /// <param name="snap">追加するシナリオデータ</param>
        public void SetNewMemento(List<Scenario> snap)
        {
            SnapShot.Push(snap);
        }
    }
}
