using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wf = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MahApps.Metro.Controls;


namespace Bve5ScenarioEditor
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : MetroWindow
    {

        List<Scenario> editData;

        void ShowScenarioInfo(List<Scenario> showData)
        {
            if (showData.Count > 0)
            {
                //選択したアイテムの共通項目を調べる
                //ベースとなるアイテムの取得
                Scenario baseScenario = showData[0];
                int imgIdx = baseScenario.Item.ImageIndex;
                string title = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.TITLE].Text;
                string routeTitle = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.ROUTE_TITLE].Text;
                string vehicleTitle = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.VEHICLE_TITLE].Text;
                string author = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.AUTHOR].Text;
                string comment = baseScenario.Data.Comment ?? "";
                string fileName = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.FILE_NAME].Text;

                //サムネイル情報の描画
                if (imgIdx != -1)
                {
                    Bitmap bitmap = (Bitmap)baseScenario.Item.ImageList.Images[baseScenario.Item.ImageIndex];
                    using (Stream st = new MemoryStream())
                    {
                        bitmap.Save(st, System.Drawing.Imaging.ImageFormat.Png);
                        st.Seek(0, SeekOrigin.Begin);
                        thumbnailImage.Source = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                //情報をTextBlockに設定
                scenarioTitleText.Text = title;
                scenarioRouteTitleText.Text = routeTitle;
                scenarioVehicleTitleText.Text = vehicleTitle;
                scenarioAuthorText.Text = author;
                scenarioCommentText.Text = comment;
                scenarioFileNameText.Text = fileName;

                //初期設定にすべての情報を表示
                thumbnailImage.Visibility = Visibility.Visible;
                scenarioTitleText.Visibility = Visibility.Visible;
                scenarioCommentText.Visibility = Visibility.Visible;
                scenarioRouteTitleText.Visibility = Visibility.Visible;
                scenarioVehicleTitleText.Visibility = Visibility.Visible;
                scenarioAuthorText.Visibility = Visibility.Visible;
                scenarioFileNameText.Visibility = Visibility.Visible;

                //ベースと異なる情報は非表示に
                foreach (Scenario scenario in showData)
                {

                    if (scenario.Item.ImageIndex != imgIdx || imgIdx == -1)
                        thumbnailImage.Visibility = Visibility.Collapsed;
                    if (!scenario.Item.SubItems[(int)Scenario.SubItemIndex.TITLE].Text.Equals(title))
                        scenarioTitleText.Text = "複数タイトル...";
                    if (!scenario.Item.SubItems[(int)Scenario.SubItemIndex.ROUTE_TITLE].Text.Equals(routeTitle))
                        scenarioRouteTitleText.Text = "複数路線名..."; ;
                    if (!scenario.Item.SubItems[(int)Scenario.SubItemIndex.VEHICLE_TITLE].Text.Equals(vehicleTitle))
                        scenarioVehicleTitleText.Text = "複数車両名...";
                    if (!scenario.Item.SubItems[(int)Scenario.SubItemIndex.AUTHOR].Text.Equals(author))
                        scenarioAuthorText.Text = "複数作者...";
                    if (scenario.Data.Comment == null || scenario.Data.Comment.Equals(""))
                        scenarioCommentText.Visibility = Visibility.Collapsed;
                    else if (!scenario.Data.Comment.Equals(comment))
                        scenarioCommentText.Text = "複数コメント...";
                    if (!scenario.Item.SubItems[(int)Scenario.SubItemIndex.FILE_NAME].Text.Equals(fileName))
                        scenarioFileNameText.Text = "複数ファイル名...";
                }
            }
        }

        /// <summary>
        /// シナリオ情報表示をすべて非表示にします。
        /// </summary>
        void ClearScenarioInfo()
        {
            //シナリオ情報を非表示にする
            thumbnailImage.Visibility = Visibility.Collapsed;
            scenarioTitleText.Visibility = Visibility.Collapsed;
            scenarioCommentText.Visibility = Visibility.Collapsed;
            scenarioRouteTitleText.Visibility = Visibility.Collapsed;
            scenarioVehicleTitleText.Visibility = Visibility.Collapsed;
            scenarioAuthorText.Visibility = Visibility.Collapsed;
            scenarioFileNameText.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public EditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウインドウをダイアログとして表示します。
        /// </summary>
        /// <param name="editData">編集するシナリオデータ</param>
        /// <returns>編集後のシナリオデータ</returns>
        public List<Scenario> ShowWindow(List<Scenario> editData)
        {
            this.Title = editData.Count > 1 ? "Edit - " + editData[0].Data.Title + " など" + editData.Count + "シナリオ" : "Edit - " + editData[0].Data.Title;
            ShowScenarioInfo(editData);

            this.ShowDialog();

            return editData;
        }
    }
}
