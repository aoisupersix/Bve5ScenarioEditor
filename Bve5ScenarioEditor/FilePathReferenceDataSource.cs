using System;
using System.ComponentModel;
using Bve5_Parsing.ScenarioGrammar;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// ファイル参照のリストビューアイテム
    /// </summary>
    class FilePathReferenceDataSource : IDataErrorInfo
    {
        string weight;

        public int OriginIdx { get; set; }
        public string FilePath { get; set; }
        public string Weight
        {
            get { return weight; }
            set
            {
                weight = value;
            }
        }
        public string Probability { get; set; }

        public string Error { get { return null; } }

        //エラーメッセージ
        public string this[string propertyName]
        {
            get
            {
                string result = null;
                switch (propertyName)
                {
                    case "Weight":
                        if (this.Weight == null) return null;

                        double ii;
                        try
                        {
                            ii = double.Parse(this.Weight);
                        }
                        catch (Exception)
                        {
                            result = "重みは係数は整数もしくは少数で入力してください。";
                            break;
                        }
                        break;
                }
                return result;
            }
        }
    }
}
