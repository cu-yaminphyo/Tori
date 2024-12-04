using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using IcsComPrint;
using IcsComDb;
using C1.C1Preview;

//2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
using IcsSSSPrint;
//2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

namespace SMTORI
{
    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
    //class prnSMTORI : PrnBase
    class prnSMTORI : PrnBaseSS
    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応
    {
        private blgSMTORI mcBsLogic;

        internal prnSMTORI(dlgPrnSetting cParentForm, string sPrgID, int nUCOD
                          , string sCCOD, int nKESN, int nKeyNo, ref PrnVal cPrnVal, string sPrgName)
            : base(cParentForm, sPrgID, nUCOD, sCCOD, nKESN, nKeyNo, ref cPrnVal, ePaperSize.A4)
        {
            //**ICS-S
            fPageRightMargin = 11;
            //**ICS-E
            mcBsLogic = new blgSMTORI();
        }

        protected override bool SetC1Body()
        {

            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
            try
            {
                prnMain();
                if (PrintDivisionLastTrans() == PrintDivisionResult.NoData)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (PrintStopped()) return true;
                throw ex;
            }
            //RenderArea cRArea = new RenderArea();
            //RenderArea cRAreab;
            //cRAreab = prnMain();
            //cRArea.Children.Add(cRAreab);
            //cC1PrnDoc.Body.Children.Add(cRArea); 
            //return true;
            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応
        
        }

        //決算期のクリア
        protected override bool SetC1Header()
        {
            StrKessanKikan = "";
            return base.SetC1Header();
        }


        #region 変数
        int nLineCnt;
        #endregion


        private RenderArea prnMain()
        {
            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
            //RenderArea cRArea = new RenderArea();
            //RenderText cRText = new RenderText();
            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

            //**>>ICS-S 2013/03/04
            //**System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
            //**pen.DashPattern = new float[] { 3, 2 };
            //**C1.C1Preview.LineDef ld = new LineDef(0.1, System.Drawing.Color.Black, System.Drawing.Color.White, pen.DashPattern);
            C1.C1Preview.LineDef fr = new LineDef(0.2f, System.Drawing.Color.Black);
            C1.C1Preview.LineDef ld = new LineDef(0.05f, System.Drawing.Color.Black);
            //**<<ICS-E
            //Global.Prn_Kind 0:取引先一覧表/1:取引先台帳

            if (Global.Prn_Kind == 0)
            {
                #region 取引先一覧表
                #region 印字内容変更に伴う修正でコメントアウト
                ////検索パターン①
                //int iCnt = 0;
                //Sel_TRCD_List();

                //try
                //{
                //    if (Global.gcDataReader.HasRows == true)
                //    {
                //        RenderTable cRTableTop = new RenderTable();

                //        //**
                //        //**cRTableTop.Width = "181.5mm";
                //        //cRTableTop.Width = "180mm";
                //        //**
                //        cRTableTop.Cols[0].Width = "12mm";
                //        cRTableTop.Cols[1].Width = "37mm";
                //        cRTableTop.Cols[2].Width = "12mm";
                //        cRTableTop.Cols[3].Width = "37mm";
                //        cRTableTop.Cols[4].Width = "85mm";
                //        cRTableTop.Cols[5].Width = "14mm";
                //        cRTableTop.Cols[6].Width = "7mm";
                //        cRTableTop.Cols[7].Width = "7mm";
                //        cRTableTop.Cols[8].Width = "14mm";
                //        cRTableTop.Cols[9].Width = "14mm";
                //        cRTableTop.Cols[10].Width = "7mm";
                //        cRTableTop.Cols[11].Width = "7mm";
                //        cRTableTop.Cols[12].Width = "14mm";

                //        //cRTableTop.Cols[0].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[1].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[2].CellStyle.Spacing.Left = "0.5mm";
                //        //cRTableTop.Cols[3].CellStyle.Spacing.Left = "0.5mm";
                //        //cRTableTop.Cols[4].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[5].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[6].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[7].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[8].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[9].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[10].CellStyle.Spacing.Left = "1mm";
                //        //cRTableTop.Cols[11].CellStyle.Spacing.Left = "1mm";
                //        cRTableTop.Style.GridLines.Top = ld;
                //        cRTableTop.Style.GridLines.Left = ld;
                //        cRTableTop.Style.GridLines.Right = ld;
                //        cRTableTop.Style.GridLines.Bottom = ld;

                //        cRTableTop.Rows[0].Height = "5mm";
                //        cRTableTop.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                        
                //        cRTableTop.Cells[0, 0].SpanCols = 2;
                //        cRTableTop.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 0].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 0].Text = "取引先ｺｰﾄﾞ";
                        
                //        cRTableTop.Cells[0, 2].SpanCols = 2;
                //        cRTableTop.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 2].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 2].Text = "取引先名称 (略称)";

                //        cRTableTop.Cells[0, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 4].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 4].Text = "取引先名称 (正式名称)";

                //        cRTableTop.Cells[0, 5].SpanCols = 2;
                //        cRTableTop.Cells[0, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 5].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 5].Text = "入力開始日";

                //        cRTableTop.Cells[0, 7].SpanCols = 2;
                //        cRTableTop.Cells[0, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 7].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 7].Text = "入力終了日";

                //        cRTableTop.Cells[0, 9].SpanCols = 2;
                //        cRTableTop.Cells[0, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 9].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 9].Text = "使用開始日";

                //        cRTableTop.Cells[0, 11].SpanCols = 2;
                //        cRTableTop.Cells[0, 11].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[0, 11].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[0, 11].Text = "使用終了日";

                //        cRTableTop.Rows[1].Style.GridLines.Top = ld;
                //        cRTableTop.Rows[1].Height = "5mm";
                //        cRTableTop.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;

                //        cRTableTop.Cells[1, 0].SpanCols = 2;
                //        cRTableTop.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 0].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 0].Text = "入金代表者";

                //        cRTableTop.Cells[1, 2].SpanCols = 2;
                //        cRTableTop.Cells[1, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 2].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 2].Text = "支払代表者";

                //        cRTableTop.Cells[1, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 4].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 4].Text = "フリガナ";

                //        cRTableTop.Cells[1, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 5].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 5].Text = "カナ";

                //        cRTableTop.Cells[1, 6].SpanCols = 2;
                //        cRTableTop.Cells[1, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 6].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 6].Text = "得意先";

                //        cRTableTop.Cells[1, 8].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 8].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 8].Text = "仕入先";

                //        cRTableTop.Cells[1, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 9].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 9].Text = "期日";

                //        cRTableTop.Cells[1, 10].SpanCols = 2;
                //        cRTableTop.Cells[1, 10].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 10].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 10].Text = "ｸﾞﾙｰﾌﾟ";

                //        cRTableTop.Cells[1, 12].Style.TextAlignHorz = AlignHorzEnum.Center;
                //        cRTableTop.Cells[1, 12].Style.GridLines.Right = ld;
                //        cRTableTop.Cells[1, 12].Text = "科目";

                //        //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応＆パフォーマンス改善
                //        //cRArea.Children.Add(cRTableTop);
                //        cC1PrnDoc.Body.Children.Add(cRTableTop);
                //        RenderTable cRTable = new RenderTable();
                //        //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応＆パフォーマンス改善

                //        nLineCnt = 0;
                //        while (Global.gcDataReader.Read())
                //        {
                //            // 財務と債務共に存在する場合、財務のデータ行は出力しない
                //            if (Global.Prn_PKind == 0 && Global.gcDataReader["TYP"].ToString() == "Z" && Global.gcDataReader["SSFLG"].ToString() == "2")
                //            {
                //                continue;
                //            }
                //            //改頁
                //            if (nLineCnt == 15)
                //            {
                //                //2013/07/16 ICS.居軒 ▼パフォーマンス改善＆分割印刷＆プレビュー対応
                //                cC1PrnDoc.Body.Children.Add(cRTable);
                //                PrintDivisionResult eRet = PrintDivisionTrans();
                //                switch (eRet)
                //                {
                //                    case PrintDivisionResult.NoDivision:            //
                //                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                //                    case PrintDivisionResult.Unreached:             //
                //                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                //                        break;
                //                    case PrintDivisionResult.Preview:
                //                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                //                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                //                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                //                        return null;
                //                }
                //                cRTable = new RenderTable();
                //                iCnt = 0;
                //                //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                //                RenderTable cRTableH = new RenderTable();

                //                //**
                //                //**cRTableH.Width = "181.5mm";
                //                //cRTableH.Width = "180mm";
                //                //**
                //                cRTableH.Cols[0].Width = "12mm";
                //                cRTableH.Cols[1].Width = "37mm";
                //                cRTableH.Cols[2].Width = "12mm";
                //                cRTableH.Cols[3].Width = "37mm";
                //                cRTableH.Cols[4].Width = "85mm";
                //                cRTableH.Cols[5].Width = "14mm";
                //                cRTableH.Cols[6].Width = "7mm";
                //                cRTableH.Cols[7].Width = "7mm";
                //                cRTableH.Cols[8].Width = "14mm";
                //                cRTableH.Cols[9].Width = "14mm";
                //                cRTableH.Cols[10].Width = "7mm";
                //                cRTableH.Cols[11].Width = "7mm";
                //                cRTableH.Cols[12].Width = "14mm";

                //                //cRTableH.Cols[0].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[1].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[2].CellStyle.Spacing.Left = "0.5mm";
                //                //cRTableH.Cols[3].CellStyle.Spacing.Left = "0.5mm";
                //                //cRTableH.Cols[4].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[5].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[6].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[7].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[8].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[9].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[10].CellStyle.Spacing.Left = "1mm";
                //                //cRTableH.Cols[11].CellStyle.Spacing.Left = "1mm";

                //                cRTableH.Style.GridLines.Top = ld;
                //                cRTableH.Style.GridLines.Left = ld;
                //                cRTableH.Style.GridLines.Right = ld;
                //                cRTableH.Style.GridLines.Bottom = ld;

                //                cRTableH.Rows[0].Height = "5mm";
                //                cRTableH.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                
                //                cRTableH.Cells[0, 0].SpanCols = 2;
                //                cRTableH.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 0].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 0].Text = "取引先ｺｰﾄﾞ";

                //                cRTableH.Cells[0, 2].SpanCols = 2;
                //                cRTableH.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 2].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 2].Text = "取引先名称 (略称)";

                //                cRTableH.Cells[0, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 4].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 4].Text = "取引先名称 (正式名称)";

                //                cRTableH.Cells[0, 5].SpanCols = 2;
                //                cRTableH.Cells[0, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 5].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 5].Text = "入力開始日";

                //                cRTableH.Cells[0, 7].SpanCols = 2;
                //                cRTableH.Cells[0, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 7].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 7].Text = "入力終了日";

                //                cRTableH.Cells[0, 9].SpanCols = 2;
                //                cRTableH.Cells[0, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 9].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 9].Text = "使用開始日";

                //                cRTableH.Cells[0, 11].SpanCols = 2;
                //                cRTableH.Cells[0, 11].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[0, 11].Style.GridLines.Right = ld;
                //                cRTableH.Cells[0, 11].Text = "使用終了日";

                //                cRTableH.Rows[1].Height = "5mm";
                //                cRTableH.Rows[1].Style.GridLines.Top = ld;
                //                cRTableH.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                
                //                cRTableH.Cells[1, 0].SpanCols = 2;
                //                cRTableH.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 0].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 0].Text = "入金代表者";

                //                cRTableH.Cells[1, 2].SpanCols = 2;
                //                cRTableH.Cells[1, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 2].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 2].Text = "支払代表者";

                //                cRTableH.Cells[1, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 4].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 4].Text = "フリガナ";

                //                cRTableH.Cells[1, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 5].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 5].Text = "カナ";

                //                cRTableH.Cells[1, 6].SpanCols = 2;
                //                cRTableH.Cells[1, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 6].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 6].Text = "得意先";

                //                cRTableH.Cells[1, 8].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 8].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 8].Text = "仕入先";

                //                cRTableH.Cells[1, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 9].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 9].Text = "期日";

                //                cRTableH.Cells[1, 10].SpanCols = 2;
                //                cRTableH.Cells[1, 10].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 10].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 10].Text = "ｸﾞﾙｰﾌﾟ";

                //                cRTableH.Cells[1, 12].Style.TextAlignHorz = AlignHorzEnum.Center;
                //                cRTableH.Cells[1, 12].Style.GridLines.Right = ld;
                //                cRTableH.Cells[1, 12].Text = "科目";
                //                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                //                //cRArea.Children.Add(cRTableH);
                //                if (cC1PrnDoc.Body.Children.Count > 0)
                //                {
                //                    cRTableH.BreakBefore = BreakEnum.Page;
                //                }
                //                cC1PrnDoc.Body.Children.Add(cRTableH);
                //                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                //                nLineCnt = 0;
                //            }

                //            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                //            //iCnt++;
                //            //RenderTable cRTable = new RenderTable();
                //            //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                //            //**
                //            //**cRTable.Width = "181.5mm";
                //            //cRTable.Width = "180mm";
                //            //**
                //            cRTable.Cols[0].Width = "12mm";
                //            cRTable.Cols[1].Width = "37mm";
                //            cRTable.Cols[2].Width = "12mm";
                //            cRTable.Cols[3].Width = "37mm";
                //            cRTable.Cols[4].Width = "85mm";
                //            cRTable.Cols[5].Width = "14mm";
                //            cRTable.Cols[6].Width = "7mm";
                //            cRTable.Cols[7].Width = "7mm";
                //            cRTable.Cols[8].Width = "14mm";
                //            cRTable.Cols[9].Width = "14mm";
                //            cRTable.Cols[10].Width = "7mm";
                //            cRTable.Cols[11].Width = "7mm";
                //            cRTable.Cols[12].Width = "14mm";

                //            //cRTable.Cols[0].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[1].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[2].CellStyle.Spacing.Left = "0.5mm";
                //            //cRTable.Cols[3].CellStyle.Spacing.Left = "0.5mm";
                //            //cRTable.Cols[4].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[5].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[6].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[7].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[8].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[9].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[10].CellStyle.Spacing.Left = "1mm";
                //            //cRTable.Cols[11].CellStyle.Spacing.Left = "1mm";

                //            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                //            //cRTable.Style.GridLines.Top = LineDef.Empty;
                //            //cRTable.Style.GridLines.Left = LineDef.Default;
                //            //cRTable.Style.GridLines.Right = LineDef.Default;
                //            //cRTable.Style.GridLines.Bottom = LineDef.Default;
                //            //cRTable.Rows[iCnt].Height = "5mm";
                //            cRTable.Style.GridLines.All = ld;
                //            cRTable.Style.GridLines.Top = LineDef.Empty;
                //            cRTable.Rows[iCnt].Height = "5.3mm";
                //            //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                //            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                //            cRTable.Rows[iCnt].Height = "5mm";
                //            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                //            cRTable.Cells[iCnt, 0].SpanCols = 2;
                //            cRTable.Cells[iCnt, 0].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 0].Style.GridLines.Right = ld;
                //            string sTRCD;
                //            if ((Global.nTRCD_Type == 0) &&
                //                (Global.nTRCD_ZE == 1))
                //            {
                //                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                //            }
                //            else if (Global.nTRCD_Type == 1)
                //            {
                //                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                //            }
                //            else
                //            {
                //                sTRCD = Global.gcDataReader["TRCD"].ToString();
                //            }
                //            if (sTRCD.Length == 13)
                //            {
                //                cRTable.Cells[iCnt, 0].Text = "";
                //            }
                //            else
                //            {
                //                if (Global.nTRCD_HJ == 1)
                //                {
                //                    if (Global.gcDataReader["HJCD"] == null || Global.gcDataReader["HJCD"] == DBNull.Value)
                //                    {
                //                        cRTable.Cells[iCnt, 0].Text = sTRCD;
                //                    }
                //                    else
                //                    {
                //                        cRTable.Cells[iCnt, 0].Text = sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0');
                //                    }
                //                }
                //                else
                //                {
                //                    cRTable.Cells[iCnt, 0].Text = sTRCD.PadRight(20);
                //                }
                //            }
                //            cRTable.Cells[iCnt, 2].SpanCols = 2;
                //            cRTable.Cells[iCnt, 2].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 2].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 2].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 2].Style.WordWrap = false;
                //            cRTable.Cells[iCnt, 2].Style.FontSize = 8;
                //            cRTable.Cells[iCnt, 2].Text = Global.gcDataReader["RYAKU"].ToString().PadRight(20);

                //            cRTable.Cells[iCnt, 4].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 4].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 4].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 4].Style.WordWrap = false;
                //            cRTable.Cells[iCnt, 4].Style.FontSize = 8;
                //            cRTable.Cells[iCnt, 4].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(44);

                //            cRTable.Cells[iCnt, 5].SpanCols = 2;
                //            cRTable.Cells[iCnt, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            if (Global.gcDataReader["ISTAYMD"].ToString() != "0" && Global.gcDataReader["ISTAYMD"].ToString() != "")
                //            {
                //                cRTable.Cells[iCnt, 5].Text = Global.gcDataReader["ISTAYMD"].ToString().Insert(6, "/").Insert(4, "/");
                //            }
                //            cRTable.Cells[iCnt, 7].SpanCols = 2;
                //            cRTable.Cells[iCnt, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            if (Global.gcDataReader["IENDYMD"].ToString() != "0" && Global.gcDataReader["IENDYMD"].ToString() != "")
                //            {
                //                cRTable.Cells[iCnt, 7].Text = Global.gcDataReader["IENDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                //            }
                //            cRTable.Cells[iCnt, 9].SpanCols = 2;
                //            cRTable.Cells[iCnt, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            if (Global.gcDataReader["STYMD"].ToString() != "0" && Global.gcDataReader["STYMD"].ToString() != "")
                //            {
                //                cRTable.Cells[iCnt, 9].Text = Global.gcDataReader["STYMD"].ToString().Insert(6, "/").Insert(4, "/");
                //            }
                //            cRTable.Cells[iCnt, 11].SpanCols = 2;
                //            cRTable.Cells[iCnt, 11].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            if (Global.gcDataReader["EDYMD"].ToString() != "0" && Global.gcDataReader["EDYMD"].ToString() != "")
                //            {
                //                cRTable.Cells[iCnt, 11].Text = Global.gcDataReader["EDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                //            }

                //            iCnt++;

                //            cRTable.Rows[iCnt].Height = "5mm";
                //            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                //            cRTable.Cells[iCnt, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 0].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 0].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 0].Text = (Global.gcDataReader["SAIKEN_FLG"].ToString() == "1" ? "入代" : "");

                //            cRTable.Cells[iCnt, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 1].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 1].Style.GridLines.Right = ld;
                //            if ((Global.nTRCD_Type == 0) &&
                //                (Global.nTRCD_ZE == 1))
                //            {
                //                sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                //            }
                //            else if (Global.nTRCD_Type == 1)
                //            {
                //                sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                //            }
                //            else
                //            {
                //                sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                //            }
                //            if (Global.nTRCD_HJ == 1)
                //            {
                //                if (Global.gcDataReader["NYDAIHJCD"] == null || Global.gcDataReader["NYDAIHJCD"] == DBNull.Value)
                //                {
                //                    cRTable.Cells[iCnt, 1].Text = sTRCD;
                //                }
                //                else
                //                {
                //                    cRTable.Cells[iCnt, 1].Text = sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0');
                //                }
                //            }
                //            else
                //            {
                //                cRTable.Cells[iCnt, 1].Text = sTRCD.PadRight(20);
                //            }

                //            cRTable.Cells[iCnt, 2].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 2].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 2].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 2].Text = (Global.gcDataReader["SAIMU_FLG"].ToString() == "1" ? "支代" : "");

                //            cRTable.Cells[iCnt, 3].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 3].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 3].Style.GridLines.Right = ld;
                //            if ((Global.nTRCD_Type == 0) &&
                //                (Global.nTRCD_ZE == 1))
                //            {
                //                sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                //            }
                //            else if (Global.nTRCD_Type == 1)
                //            {
                //                sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                //            }
                //            else
                //            {
                //                sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                //            }
                //            if (Global.nTRCD_HJ == 1)
                //            {
                //                if (Global.gcDataReader["SIDAIHJCD"] == null || Global.gcDataReader["SIDAIHJCD"] == DBNull.Value)
                //                {
                //                    cRTable.Cells[iCnt, 3].Text = sTRCD;
                //                }
                //                else
                //                {
                //                    cRTable.Cells[iCnt, 3].Text = sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0');
                //                }
                //            }
                //            else
                //            {
                //                cRTable.Cells[iCnt, 3].Text = sTRCD.PadRight(20);
                //            }

                //            cRTable.Cells[iCnt, 4].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 4].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 4].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 4].Style.WordWrap = false;
                //            cRTable.Cells[iCnt, 4].Style.FontSize = 8;
                //            cRTable.Cells[iCnt, 4].Text = Global.gcDataReader["TRFURI"].ToString().PadRight(44);

                //            cRTable.Cells[iCnt, 5].Style.TextAlignHorz = AlignHorzEnum.Left;
                //            cRTable.Cells[iCnt, 5].CellStyle.Spacing.Left = "1mm";
                //            cRTable.Cells[iCnt, 5].Style.GridLines.Right = ld;
                //            cRTable.Cells[iCnt, 5].Style.WordWrap = false;
                //            cRTable.Cells[iCnt, 5].Style.FontSize = 8;
                //            cRTable.Cells[iCnt, 5].Text = Global.gcDataReader["KNLD"].ToString().PadRight(4);

                //            cRTable.Cells[iCnt, 6].SpanCols = 2;
                //            cRTable.Cells[iCnt, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            cRTable.Cells[iCnt, 6].Text = (Global.gcDataReader["SAIKEN"].ToString() == "1" ? "○" : "");

                //            cRTable.Cells[iCnt, 8].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            cRTable.Cells[iCnt, 8].Text = (Global.gcDataReader["SAIMU"].ToString() == "1" ? "○" : "");

                //            cRTable.Cells[iCnt, 9].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            cRTable.Cells[iCnt, 9].Text = (Global.gcDataReader["TGASW"].ToString() == "1" ? "○" : "");

                //            cRTable.Cells[iCnt, 10].SpanCols = 2;
                //            cRTable.Cells[iCnt, 10].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            cRTable.Cells[iCnt, 10].Text = Global.gcDataReader["GRPID"].ToString().PadLeft(2);

                //            cRTable.Cells[iCnt, 12].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            cRTable.Cells[iCnt, 12].Text = (Global.gcDataReader["KMK"].ToString() == "1" ? "○" : "");

                //            //cRTable.Cells[iCnt, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            //cRTable.Cells[iCnt, 4].Text = (Global.gcDataReader["ZFLG"].ToString() == "1" ? "○" : "×");
                //            //cRTable.Cells[iCnt, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                //            //cRTable.Cells[iCnt, 5].Text = (Global.gcDataReader["SSFLG"].ToString() == "1" || Global.gcDataReader["SSFLG"].ToString() == "2" ? "○" : "×");

                //            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                //            //cRArea.Children.Add(cRTable);
                //            iCnt++;
                //            //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                //            nLineCnt++; //出力行数カウントアップ
                //        }

                //        //2013/07/16 ICS.居軒 ▼パフォーマンス改善＆分割印刷＆プレビュー対応
                //        cC1PrnDoc.Body.Children.Add(cRTable);
                //        //2013/07/16 ICS.居軒 ▲パフォーマンス改善＆分割印刷＆プレビュー対応

                //    }
                //}
                //finally
                //{
                //    if (Global.gcDataReader != null)
                //    {
                //        Global.gcDataReader.Close();
                //        Global.gcDataReader.Dispose();
                //    }
                //}
                #endregion
//-- <2016/03/15 新規作成>
                //検索パターン①
                int iCnt = 0;
                Sel_TRCD_List();

                try
                {
                    if (Global.gcDataReader.HasRows == true)
                    {
                        RenderTable cRTableTop = new RenderTable();

                        cRTableTop.Cols[0].Width = "49mm";
                        cRTableTop.Cols[1].Width = "85mm";
                        cRTableTop.Cols[2].Width = "12mm";
                        cRTableTop.Cols[3].Width = "37mm";
                        cRTableTop.Cols[4].Width = "21mm";
                        cRTableTop.Cols[5].Width = "21mm";
                        cRTableTop.Cols[6].Width = "21mm";
                        cRTableTop.Cols[7].Width = "21mm";

                        cRTableTop.Style.GridLines.Top = fr;
                        cRTableTop.Style.GridLines.Left = fr;
                        cRTableTop.Style.GridLines.Right = fr;
                        cRTableTop.Style.GridLines.Bottom = ld;

                        cRTableTop.Rows[0].Height = "5mm";
                        cRTableTop.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;

                        cRTableTop.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 0].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 0].Text = "取引先ｺｰﾄﾞ";

                        cRTableTop.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 1].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 1].Text = "取引先名称 (正式名称)";

                        // Ver.01.09.02 [SIAS-7540] Toda -->
                        //cRTableTop.Cells[0, 2].SpanCols = 2;
                        //cRTableTop.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                        //cRTableTop.Cells[0, 2].Style.GridLines.Right = ld;
                        //cRTableTop.Cells[0, 2].Text = "入金代表者";
                        cRTableTop.Cells[0, 2].Style.GridLines.Right = ld;

                        cRTableTop.Cells[0, 3].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 3].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 3].Text = "入金代表者";
                        // Ver.01.09.02 <--

                        cRTableTop.Cells[0, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 4].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 4].Text = "得意先";

                        cRTableTop.Cells[0, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 5].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 5].Text = "仕入先";

                        cRTableTop.Cells[0, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 6].Style.GridLines.Right = ld;
                        cRTableTop.Cells[0, 6].Text = "使用開始日";

                        cRTableTop.Cells[0, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[0, 7].Style.GridLines.Right = fr;
                        cRTableTop.Cells[0, 7].Text = "使用終了日";

                        cRTableTop.Rows[1].Style.GridLines.Top = ld;
                        cRTableTop.Rows[1].Height = "5mm";
                        cRTableTop.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;

                        cRTableTop.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 0].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 0].Text = "取引先名称 (略称)";

                        cRTableTop.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 1].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 1].Text = "フリガナ";

                        cRTableTop.Cells[1, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 2].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 2].Text = "カナ";

                        cRTableTop.Cells[1, 3].Style.GridLines.Right = ld;
                        // Ver.01.09.02 [SIAS-7540] Toda -->
                        cRTableTop.Cells[1, 3].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 3].Text = "支払代表者";
                        // Ver.01.09.02 <--

                        cRTableTop.Cells[1, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 4].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 4].Text = "ｸﾞﾙｰﾌﾟ";

                        cRTableTop.Cells[1, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 5].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 5].Text = "科目";

                        cRTableTop.Cells[1, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 6].Style.GridLines.Right = ld;
                        cRTableTop.Cells[1, 6].Text = "入力開始日";

                        cRTableTop.Cells[1, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                        cRTableTop.Cells[1, 7].Style.GridLines.Right = fr;
                        cRTableTop.Cells[1, 7].Text = "入力終了日";

                        cRTableTop.Style.GridLines.Bottom = fr;

                        cC1PrnDoc.Body.Children.Add(cRTableTop);
                        RenderTable cRTable = new RenderTable();

                        nLineCnt = 0;
                        while (Global.gcDataReader.Read())
                        {
                            // 財務と債務共に存在する場合、財務のデータ行は出力しない
                            if (Global.Prn_PKind == 0 && Global.gcDataReader["TYP"].ToString() == "Z" && Global.gcDataReader["SSFLG"].ToString() == "2")
                            {
                                continue;
                            }
                            //改頁
                            if (nLineCnt == 15)
                            {
                                cC1PrnDoc.Body.Children.Add(cRTable);
                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                cRTable = new RenderTable();
                                iCnt = 0;

                                RenderTable cRTableH = new RenderTable();

                                cRTableH.Cols[0].Width = "49mm";
                                cRTableH.Cols[1].Width = "85mm";
                                cRTableH.Cols[2].Width = "12mm";
                                cRTableH.Cols[3].Width = "37mm";
                                cRTableH.Cols[4].Width = "21mm";
                                cRTableH.Cols[5].Width = "21mm";
                                cRTableH.Cols[6].Width = "21mm";
                                cRTableH.Cols[7].Width = "21mm";

                                cRTableH.Style.GridLines.Top = fr;
                                cRTableH.Style.GridLines.Left = fr;
                                cRTableH.Style.GridLines.Right = fr;
                                cRTableH.Style.GridLines.Bottom = ld;

                                cRTableH.Rows[0].Height = "5mm";
                                cRTableH.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTableH.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 0].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 0].Text = "取引先ｺｰﾄﾞ";

                                cRTableH.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 1].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 1].Text = "取引先名称 (正式名称)";

                                // Ver.01.09.02 [SIAS-7540] Toda -->
                                //cRTableH.Cells[0, 2].SpanCols = 2;
                                //cRTableH.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                //cRTableH.Cells[0, 2].Style.GridLines.Right = ld;
                                //cRTableH.Cells[0, 2].Text = "入金代表者";
                                cRTableH.Cells[0, 2].Style.GridLines.Right = ld;

                                cRTableH.Cells[0, 3].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 3].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 3].Text = "入金代表者";
                                // Ver.01.09.02 <--

                                cRTableH.Cells[0, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 4].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 4].Text = "得意先";

                                cRTableH.Cells[0, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 5].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 5].Text = "仕入先";

                                cRTableH.Cells[0, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 6].Style.GridLines.Right = ld;
                                cRTableH.Cells[0, 6].Text = "使用開始日";

                                cRTableH.Cells[0, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[0, 7].Style.GridLines.Right = fr;
                                cRTableH.Cells[0, 7].Text = "使用終了日";

                                cRTableH.Rows[1].Height = "5mm";
                                cRTableH.Rows[1].Style.GridLines.Top = ld;
                                cRTableH.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTableH.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 0].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 0].Text = "取引先名称 (略称)";

                                cRTableH.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 1].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 1].Text = "フリガナ";

                                cRTableH.Cells[1, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 2].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 2].Text = "カナ";

                                cRTableH.Cells[1, 3].Style.GridLines.Right = ld;
                                // Ver.01.09.02 [SIAS-7540] Toda -->
                                cRTableH.Cells[1, 3].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 3].Text = "支払代表者";
                                // Ver.01.09.02 <--

                                cRTableH.Cells[1, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 4].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 4].Text = "ｸﾞﾙｰﾌﾟ";

                                cRTableH.Cells[1, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 5].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 5].Text = "科目";

                                cRTableH.Cells[1, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 6].Style.GridLines.Right = ld;
                                cRTableH.Cells[1, 6].Text = "入力開始日";

                                cRTableH.Cells[1, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTableH.Cells[1, 7].Style.GridLines.Right = fr;
                                cRTableH.Cells[1, 7].Text = "入力終了日";

                                cRTableH.Style.GridLines.Bottom = fr;

                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTableH.BreakBefore = BreakEnum.Page;
                                }
                                cC1PrnDoc.Body.Children.Add(cRTableH);

                                nLineCnt = 0;
                            }

                            cRTable.Cols[0].Width = "49mm";
                            cRTable.Cols[1].Width = "85mm";
                            cRTable.Cols[2].Width = "12mm";
                            cRTable.Cols[3].Width = "37mm";
                            cRTable.Cols[4].Width = "21mm";
                            cRTable.Cols[5].Width = "21mm";
                            cRTable.Cols[6].Width = "21mm";
                            cRTable.Cols[7].Width = "21mm";

                            cRTable.Style.GridLines.All = ld;

                            cRTable.Style.GridLines.Left = fr;
                            cRTable.Style.GridLines.Right = fr;
//                            cRTable.Style.GridLines.Bottom = ld;
//                            cRTable.Style.GridLines = ld;

                            cRTable.Style.GridLines.Top = LineDef.Empty;
                            cRTable.Rows[iCnt].Height = "5.3mm";

                            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                            cRTable.Rows[iCnt].Height = "5mm";
                            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                            cRTable.Cells[iCnt, 0].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cells[iCnt, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 0].Style.GridLines.Right = ld;
                            string sTRCD;
                            if ((Global.nTRCD_Type == 0) &&
                                (Global.nTRCD_ZE == 1))
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                            }
                            else if (Global.nTRCD_Type == 1)
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                            }
                            else
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString();
                            }
                            if (sTRCD.Length == 13)
                            {
                                cRTable.Cells[iCnt, 0].Text = "";
                            }
                            else
                            {
                                if (Global.nTRCD_HJ == 1)
                                {
                                    if (Global.gcDataReader["HJCD"] == null || Global.gcDataReader["HJCD"] == DBNull.Value)
                                    {
                                        cRTable.Cells[iCnt, 0].Text = sTRCD;
                                    }
                                    else
                                    {
                                        cRTable.Cells[iCnt, 0].Text = sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0');
                                    }
                                }
                                else
                                {
                                    cRTable.Cells[iCnt, 0].Text = sTRCD.PadRight(20);
                                }
                            }

                            cRTable.Cells[iCnt, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 1].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cells[iCnt, 1].Style.GridLines.Right = ld;
                            cRTable.Cells[iCnt, 1].Style.WordWrap = false;
                            cRTable.Cells[iCnt, 1].Style.FontSize = 8;
                            cRTable.Cells[iCnt, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(44);

                            // Ver.01.09.02 [SIAS-7540] Toda -->
                            //cRTable.Cells[iCnt, 2].Style.TextAlignHorz = AlignHorzEnum.Left;
                            //cRTable.Cells[iCnt, 2].CellStyle.Spacing.Left = "1mm";
                            //cRTable.Cells[iCnt, 2].Style.GridLines.Right = ld;
                            //cRTable.Cells[iCnt, 2].Text = (Global.gcDataReader["SAIKEN_FLG"].ToString() == "1" ? "入代" : "");
                            cRTable.Cells[iCnt, 2].Style.GridLines.Right = ld;
                            // Ver.01.09.02 <--

                            cRTable.Cells[iCnt, 3].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 3].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cells[iCnt, 3].Style.GridLines.Right = ld;
                            // Ver.01.09.02 [SIAS-7540] Toda -->
                            if (Global.gcDataReader["SAIKEN_FLG"].ToString() == "1")
                            {
                                cRTable.Cells[iCnt, 3].Text = "入金代表者";
                            }
                            else
                            {
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
                                if (Global.nTRCD_HJ == 1)
                                {
                                    if (Global.gcDataReader["NYDAIHJCD"] == null || Global.gcDataReader["NYDAIHJCD"] == DBNull.Value)
                                    {
                                        cRTable.Cells[iCnt, 3].Text = sTRCD;
                                    }
                                    else
                                    {
                                        cRTable.Cells[iCnt, 3].Text = sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0');
                                    }
                                }
                                else
                                {
                                    cRTable.Cells[iCnt, 3].Text = sTRCD.PadRight(20);
                                }
                            }

                            cRTable.Cells[iCnt, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable.Cells[iCnt, 4].Text = (Global.gcDataReader["SAIKEN"].ToString() == "1" ?
                                "○" : 
                                (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2" ? 
                                    "期日のみ" : 
                                    ""));

                            cRTable.Cells[iCnt, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable.Cells[iCnt, 5].Text = (Global.gcDataReader["SAIMU"].ToString() == "1" ? 
                                "○" :
                                (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3" ? 
                                    "期日のみ" : 
                                    ""));

                            cRTable.Cells[iCnt, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                            if (Global.gcDataReader["STYMD"].ToString() != "0" && Global.gcDataReader["STYMD"].ToString() != "")
                            {
                                cRTable.Cells[iCnt, 6].Text = Global.gcDataReader["STYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }

                            cRTable.Cells[iCnt, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                            if (Global.gcDataReader["EDYMD"].ToString() != "0" && Global.gcDataReader["EDYMD"].ToString() != "")
                            {
                                cRTable.Cells[iCnt, 7].Text = Global.gcDataReader["EDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }
                            
                            iCnt++;

                            cRTable.Rows[iCnt].Height = "5mm";
                            cRTable.Rows[iCnt].Style.TextAlignVert = AlignVertEnum.Center;

                            cRTable.Cells[iCnt, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 0].CellStyle.Spacing.Left = "5mm";
                            cRTable.Cells[iCnt, 0].Style.GridLines.Right = ld;
                            cRTable.Cells[iCnt, 0].Style.WordWrap = false;
                            cRTable.Cells[iCnt, 0].Style.FontSize = 8;
                            cRTable.Cells[iCnt, 0].Text = Global.gcDataReader["RYAKU"].ToString().PadRight(20);

                            cRTable.Cells[iCnt, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 1].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cells[iCnt, 1].Style.GridLines.Right = ld;
                            cRTable.Cells[iCnt, 1].Style.WordWrap = false;
                            cRTable.Cells[iCnt, 1].Style.FontSize = 8;
                            cRTable.Cells[iCnt, 1].Text = Global.gcDataReader["TRFURI"].ToString().PadRight(44);

                            cRTable.Cells[iCnt, 2].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 2].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cells[iCnt, 2].Style.GridLines.Right = ld;
                            cRTable.Cells[iCnt, 2].Style.WordWrap = false;
                            cRTable.Cells[iCnt, 2].Style.FontSize = 8;
                            cRTable.Cells[iCnt, 2].Text = Global.gcDataReader["KNLD"].ToString().PadRight(4);

                            cRTable.Cells[iCnt, 3].Style.GridLines.Right = ld;
                            // Ver.01.09.02 [SIAS-7540] Toda -->
                            cRTable.Cells[iCnt, 3].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[iCnt, 3].CellStyle.Spacing.Left = "1mm";
                            if (Global.gcDataReader["SAIMU_FLG"].ToString() == "1")
                            {
                                cRTable.Cells[iCnt, 3].Text = "支払代表者";
                            }
                            else
                            {
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
                                if (Global.nTRCD_HJ == 1)
                                {
                                    if (Global.gcDataReader["SIDAIHJCD"] == null || Global.gcDataReader["SIDAIHJCD"] == DBNull.Value)
                                    {
                                        cRTable.Cells[iCnt, 3].Text = sTRCD;
                                    }
                                    else
                                    {
                                        cRTable.Cells[iCnt, 3].Text = sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0');
                                    }
                                }
                                else
                                {
                                    cRTable.Cells[iCnt, 3].Text = sTRCD.PadRight(20);
                                }
                            }
                            // Ver.01.09.02 <--

                            cRTable.Cells[iCnt, 4].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable.Cells[iCnt, 4].Text = Global.gcDataReader["GRPID"].ToString();

                            cRTable.Cells[iCnt, 5].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable.Cells[iCnt, 5].Text = (Global.gcDataReader["KMK"].ToString() == "1" ? "○" : "");

                            cRTable.Cells[iCnt, 6].Style.TextAlignHorz = AlignHorzEnum.Center;
                            if (Global.gcDataReader["ISTAYMD"].ToString() != "0" && Global.gcDataReader["ISTAYMD"].ToString() != "")
                            {
                                cRTable.Cells[iCnt, 6].Text = Global.gcDataReader["ISTAYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }
              
                            cRTable.Cells[iCnt, 7].Style.TextAlignHorz = AlignHorzEnum.Center;
                            if (Global.gcDataReader["IENDYMD"].ToString() != "0" && Global.gcDataReader["IENDYMD"].ToString() != "")
                            {
                                cRTable.Cells[iCnt, 7].Text = Global.gcDataReader["IENDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }

                            cRTable.Rows[iCnt].Style.GridLines.Bottom = fr;

                            iCnt++;

                            nLineCnt++; //出力行数カウントアップ
                        }

                        cC1PrnDoc.Body.Children.Add(cRTable);

                    }
                }
                finally
                {
                    if (Global.gcDataReader != null)
                    {
                        Global.gcDataReader.Close();
                        Global.gcDataReader.Dispose();
                    }
                }
                #endregion
            }
            else
            {
                #region 取引先台帳
                //検索パターン②
                int iCnt;
                Sel_TRCD_Info(out iCnt);  //出力対象の取引先ｺｰﾄﾞ・補助CDの組み合わせを取得

                //取引先ｺｰﾄﾞ・補助CDの件数分loop
                nLineCnt = 0;
                for (int i = 0; i < iCnt; i++)
                {
                    //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                    Global.gcDataReader.Read();
                    //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                    RenderTable cRTableFuri = new RenderTable();

                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応＆改ページ周り不具合修正
                    //int nZanLine = 0;
                    //if (Global.Prn_Address == 0)
                    //    nZanLine = 4;
                    //else if (Global.Prn_Frigin == 0)
                    //    nZanLine = Sel_Grp2_Info_Pre(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]) * 5;
                    //else if (Global.Prn_Shiharai == 0)
                    //    nZanLine = Sel_Grp3_Info_Pre(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]) * 7;
                    //else if (Global.Prn_Bank == 0)
                    //    nZanLine = Sel_Grp4_Info_Pre(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]) * 6;
                    //else if (Global.Prn_Koujyo == 0)
                    //    nZanLine = 3;
                    //else if (Global.Prn_Others == 0)
                    //    nZanLine = 2;
                    //else if (Global.Prn_Master == 0)
                    //    nZanLine = 1;
                    int nZanLine = 0;
                    int nOneDataRowCnt = 0;
                    if (Global.Prn_Address == 0)
                    {
                        //nZanLine = 4;
                        //nOneDataRowCnt = 4;
                        nZanLine = 9;
                        nOneDataRowCnt = 9;
                    }
                    if (nZanLine == 0 && Global.Prn_Kaisyu == 0)
                    {
                        nZanLine = 17;
                        nOneDataRowCnt = 17;
                    }
                    if (nZanLine == 0 && Global.Prn_Shiharai == 0)
                    {
                        nZanLine = Sel_Grp3_Info_Pre(Global.gcDataReader["TRCD"].ToString(), Global.gcDataReader["HJCD"].ToString()) * 11;
                        nOneDataRowCnt = 11;
                    }
                    if (nZanLine == 0 && Global.Prn_Frigin == 0)
                    {
                        nZanLine = Sel_Grp2_Info_Pre(Global.gcDataReader["TRCD"].ToString(), Global.gcDataReader["HJCD"].ToString()) * 8;
                        nOneDataRowCnt = 8;
                    }
                    if (nZanLine == 0 && Global.Prn_Others == 0)
                    {
                        nZanLine = 10;
                        nOneDataRowCnt = 10;
                    }
                    if (nZanLine == 0 && Global.Prn_Gaika == 0)
                    {
                        nZanLine = 11;
                        nOneDataRowCnt = 11;
                    }
                    if (nZanLine == 0 && Global.Prn_Master == 0)
                    {
                        nZanLine = 2;
                        nOneDataRowCnt = 2;
                    }
                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応＆改ページ周り不具合修正

                    //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                    ////ヘッダー用のデータを取得
                    //Sel_Header_Info(Global.sTRCDArray[i,0], Global.sTRCDArray[i,1]);
                    //if (Global.gcDataReader.HasRows == true)
                    //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                    {
                        RenderTable cRTable = new RenderTable();

                        //改ページ
                        if ((Global.Prn_PagingTRCD == 0) &&
                            (i != 0))
                        {

                            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                            //cRTable.BreakBefore = BreakEnum.Page;
                            PrintDivisionResult eRet = PrintDivisionTrans();
                            switch (eRet)
                            {
                                case PrintDivisionResult.NoDivision:            //
                                case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                case PrintDivisionResult.Unreached:             //
                                case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                    break;
                                case PrintDivisionResult.Preview:
                                case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                    return null;
                            }
                            if (cC1PrnDoc.Body.Children.Count > 0)
                            {
                                cRTable.BreakBefore = BreakEnum.Page;
                            }
                            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                            nLineCnt = 0;
                        }

                        //2013/07/16 ICS.居軒 ▼改ページ周り不具合修正
                        //振込先データが２頁以上に及び、かつ処理中ページに振込先が１データ以上出力可能のときは改ページを行わないのでヘッダー(取引先名称部)を出力する。
                        //if ((nLineCnt == 0) || (nLineCnt + 3 + nZanLine < 57))
                        //if ((nLineCnt == 0) || (nLineCnt + 3 + nZanLine < 57) || ((nLineCnt + 3 + nOneDataRowCnt < 57) && (nZanLine + 3 >= 62)))
                        if ((nLineCnt == 0) || (nLineCnt + 3 + nOneDataRowCnt < 62))    //←★処理中ページに１データでも入るなら改ページせず、ヘッダー(取引先名称部)を出力するするよう修正。
                        //2013/07/16 ICS.居軒 ▲改ページ周り不具合修正
                        {
                            cRTable.Rows[0].Height = "6mm";
                            cRTable.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable.Rows[1].Height = "4mm";
                            cRTable.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable.Rows[2].Height = "4mm";
                            cRTable.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;
                            
                            cRTable.Cols[0].Width = "35mm";
                            cRTable.Cols[1].Width = "110mm";
                            cRTable.Cols[2].Width = "35mm";

                            cRTable.Cols[0].CellStyle.Spacing.Left = "1mm";
                            cRTable.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTable.Style.GridLines.Top = fr;
                            cRTable.Style.GridLines.Left = fr;
                            cRTable.Style.GridLines.Right = fr;
                            cRTable.Style.GridLines.Bottom = fr;
                            string sWork;

                            //ヘッダー1行目
                            cRTable.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                            string sTRCD;
                            if ((Global.nTRCD_Type == 0) &&
                                (Global.nTRCD_ZE == 1))
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                            }
                            else if (Global.nTRCD_Type == 1)
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                            }
                            else
                            {
                                sTRCD = Global.gcDataReader["TRCD"].ToString();
                            }
                            if (sTRCD.Length == 13)
                            {
                                cRTable.Cells[0, 0].Text = "";
                            }
                            else
                            {
                                cRTable.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                            }
                            cRTable.Cells[0, 1].Style.FontSize = 12;
                            cRTable.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                            cRTable.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                            cRTable.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                            //ヘッダー2行目
                            cRTable.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                            sWork = "得意先:";
                            if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                            {
                                sWork += "○";
                            }
                            else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                            {
                                sWork += "期日管理のみ";
                            }
                            else
                            {
                                sWork += "－";
                            }
                            cRTable.Cells[1, 0].Text = sWork;

                            cRTable.Cells[1, 1].SpanCols = 2;
                            cRTable.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14 文字連結変更>
//                            string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " " 
//                                         + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                         + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                            sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                  + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                  + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <>
                            if ((Global.nTRCD_Type == 0) &&
                                (Global.nTRCD_ZE == 1))
                            {
                                sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                            }
                            else if (Global.nTRCD_Type == 1)
                            {
                                sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                            }
                            else
                            {
                                sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                            }
//-- <2016/03/14 取引先が無いのであれば補助コードも要らない。取引先コードがあるのであれば項目ヘッダーも要る>
//                            sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                            if (sTRCD != "")
                            {
                                sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                            }
//-- <2016/03/14>
                            cRTable.Cells[1, 1].Text = sWork;

                            cRTable.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                            sWork = "仕入先:";
                            if (Global.gcDataReader["SAIMU"].ToString() == "1")
                            {
                                sWork += "○";
                            }
                            else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                            {
                                sWork += "期日管理のみ";
                            }
                            else
                            {
                                sWork += "－";
                            }
                            cRTable.Cells[2, 0].Text = sWork;

                            cRTable.Cells[2, 1].SpanCols = 2;
                            cRTable.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                            sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                            if (Global.gcDataReader["GRPID"].ToString() != "0")
                            {
                                sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                      + "    "
//-- <2016/03/14 取引先が無いのであれば、補助コードも要らない。あれば項目ヘッダー付きで判断する>
//                                      + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                      + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                            }
                            else
                            {
                                sWork += " ".PadRight(23, ' ')
                                      + "    "
//-- <2016/03/14>
//                                      + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                      + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                            }

                            if ((Global.nTRCD_Type == 0) &&
                                (Global.nTRCD_ZE == 1))
                            {
                                sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                            }
                            else if (Global.nTRCD_Type == 1)
                            {
                                sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                            }
                            else
                            {
                                sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                            }
//-- <2016/03/14>
//                            sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                            if (sTRCD != "")
                            {
                                sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                            }
//-- <2016/03/14>

                            cRTable.Cells[2, 1].Text = sWork;

                            //string sNAYOSE = (Global.gcDataReader["NAYOSE"].ToString() == "0" ? "×" : "○");
                            //string sSETUIN = (Global.gcDataReader["F_SETUIN"].ToString() == "0" ? "×" : "○");
                            //string sTGASW = (Global.gcDataReader["TGASW"].ToString() == "0" ? "                  " : "手形管理のみで使用");
                            //cRTable.Cells[1, 1].Text = Global.gcDataReader["RYAKU"].ToString().PadRight(10, '　').Substring(0, 10) + "    "
                            //                         + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "     "
                            //                         + "名寄せ" + sNAYOSE + "     " + "節印実行" + sSETUIN + "  "
                            //                         + sTGASW;
                            nLineCnt = nLineCnt + 3;
                            cRTableFuri = (RenderTable)cRTable.Clone();

                            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                            //cRArea.Children.Add(cRTable);
                            cC1PrnDoc.Body.Children.Add(cRTable);
                            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                        }
                        nLineCnt = nLineCnt + 3;

                        //住所等がチェックされていた場合、出力
                        if (Global.Prn_Address == 0)
                        {
                            //if (nLineCnt + 4 >= 62)
                            if (nLineCnt + 9 >= 62)
                            {
                                //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                                //Sel_Header_Info(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]);
                                //if (Global.gcDataReader.HasRows == true)
                                //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                                {
                                    //改頁前の下線補完
                                    RenderTable cRTable6 = new RenderTable();
                                    //**
                                    //**cRTable6.Width = "181.5mm";
                                    //cRTable6.Width = "180mm";
                                    //**

                                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                    //cRArea.Children.Add(cRTable6);
                                    cC1PrnDoc.Body.Children.Add(cRTable6);
                                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                    RenderTable cRTable_H = new RenderTable();
                                    //改頁を挿入

                                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                    //cRTable_H.BreakBefore = BreakEnum.Page;
                                    PrintDivisionResult eRet = PrintDivisionTrans();
                                    switch (eRet)
                                    {
                                        case PrintDivisionResult.NoDivision:            //
                                        case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                        case PrintDivisionResult.Unreached:             //
                                        case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                            break;
                                        case PrintDivisionResult.Preview:
                                        case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                        case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                        case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                            return null;
                                    }
                                    if (cC1PrnDoc.Body.Children.Count > 0)
                                    {
                                        cRTable_H.BreakBefore = BreakEnum.Page;
                                    }
                                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                    cRTable_H.Rows[0].Height = "6mm";
                                    cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                    cRTable_H.Rows[1].Height = "4mm";
                                    cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                    cRTable_H.Rows[2].Height = "4mm";
                                    cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;
                                    
                                    cRTable_H.Cols[0].Width = "35mm";
                                    cRTable_H.Cols[1].Width = "110mm";
                                    cRTable_H.Cols[2].Width = "35mm"; 

                                    cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                    cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                    cRTable_H.Style.GridLines.Top = fr;
                                    cRTable_H.Style.GridLines.Left = fr;
                                    cRTable_H.Style.GridLines.Right = fr;
                                    cRTable_H.Style.GridLines.Bottom = fr;
                                    string sWork;

                                    //ヘッダー1行目
                                    cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                    string sTRCD = "";
                                    if ((Global.nTRCD_Type == 0) &&
                                        (Global.nTRCD_ZE == 1))
                                    {
                                        sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                    }
                                    else if (Global.nTRCD_Type == 1)
                                    {
                                        sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                    }
                                    else
                                    {
                                        sTRCD = Global.gcDataReader["TRCD"].ToString();
                                    }
                                    if (sTRCD.Length == 13)
                                    {
                                        cRTable_H.Cells[0, 0].Text = "";
                                    }
                                    else
                                    {
                                        cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                    }
                                    cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                    cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                    cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                    cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                    cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                    //ヘッダー2行目
                                    cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                    sWork = "得意先:";
                                    if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                    {
                                        sWork += "○";
                                    }
                                    else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                    {
                                        sWork += "期日管理のみ";
                                    }
                                    else
                                    {
                                        sWork += "－";
                                    }
                                    cRTable_H.Cells[1, 0].Text = sWork;

                                    cRTable_H.Cells[1, 1].SpanCols = 2;
                                    cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14 文字数を考慮>                                    
//                                    string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                 + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                 + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                    sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                          + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                          + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                    if ((Global.nTRCD_Type == 0) &&
                                        (Global.nTRCD_ZE == 1))
                                    {
                                        sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                    }
                                    else if (Global.nTRCD_Type == 1)
                                    {
                                        sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                    }
                                    else
                                    {
                                        sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                    }
//-- <2016/03/14>
//                                    sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                    if (sTRCD != "")
                                    {
                                        sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                    }
//-- <2016/03/14>
                                    cRTable_H.Cells[1, 1].Text = sWork;

                                    cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                    sWork = "仕入先:";
                                    if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                    {
                                        sWork += "○";
                                    }
                                    else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                    {
                                        sWork += "期日管理のみ";
                                    }
                                    else
                                    {
                                        sWork += "－";
                                    }
                                    cRTable_H.Cells[2, 0].Text = sWork;

                                    cRTable_H.Cells[2, 1].SpanCols = 2;
                                    cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                    sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                    if (Global.gcDataReader["GRPID"].ToString() != "0")
                                    {
                                        sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                              + "    "
//-- <2016/03/14>
//                                              + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                              + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                    }
                                    else
                                    {
                                        sWork += " ".PadRight(23, ' ')
                                              + "    "
//-- <2016/03/14>
//                                              + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                              +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                    }

                                    if ((Global.nTRCD_Type == 0) &&
                                        (Global.nTRCD_ZE == 1))
                                    {
                                        sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                    }
                                    else if (Global.nTRCD_Type == 1)
                                    {
                                        sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                    }
                                    else
                                    {
                                        sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                    }
//-- <2016/03/14>
//                                    sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                    if (sTRCD != "")
                                    {
                                        sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                    }
                                    cRTable_H.Cells[2, 1].Text = sWork;

                                    //string sNAYOSE = (Global.gcDataReader["NAYOSE"].ToString() == "0" ? "×" : "○");
                                    //string sSETUIN = (Global.gcDataReader["F_SETUIN"].ToString() == "0" ? "×" : "○");
                                    //string sTGASW = (Global.gcDataReader["TGASW"].ToString() == "0" ? "                  " : "手形管理のみで使用");
                                    //cRTable_H.Cells[1, 1].Text = Global.gcDataReader["RYAKU"].ToString().PadRight(10, '　').Substring(0, 10) + "    "
                                    //                         + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "     "
                                    //                         + "名寄せ" + sNAYOSE + "     " + "節印実行" + sSETUIN + "  "
                                    //                         + sTGASW;
                                    nLineCnt = 3;
                                    cRTableFuri = (RenderTable)cRTable_H.Clone();

                                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                    //cRArea.Children.Add(cRTable_H);
                                    cC1PrnDoc.Body.Children.Add(cRTable_H);
                                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応
                                    
                                }
                            }

                            //住所等タブのデータを取得
                            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                            //Sel_Grp1_Info(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]);
                            //if (Global.gcDataReader.HasRows == true)
                            //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                            {
                                RenderTable cRTable2 = new RenderTable();

                                //**
                                //**cRTable2.Width = "181.5mm";
                                //cRTable2.Width = "180mm";
                                //**
                                for (int j = 0; j < 9; j++)
                                {
                                    cRTable2.Rows[j].Height = "4mm";
                                    cRTable2.Rows[j].Style.TextAlignVert = AlignVertEnum.Center;
                                }
                                cRTable2.Cols[0].Width = "20mm";
                                //**
                                //**cRTable2.Cols[1].Width = "161.5mm";
                                cRTable2.Cols[1].Width = "160mm";
                                //**
                                cRTable2.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable2.Style.GridLines.All = fr;
                                cRTable2.Style.GridLines.Top = LineDef.Empty;
                                //住所等タイトル
                                cRTable2.Cells[0, 0].SpanRows = 9;
                                cRTable2.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable2.Cells[0, 0].Text = "基本設定";
                                //住所等1行目
                                string sZIP = "";
                                if (Global.gcDataReader["ZIP"].ToString().Length == 7)
                                {
                                    sZIP = Global.gcDataReader["ZIP"].ToString().Substring(0, 3) + "-"
                                         + Global.gcDataReader["ZIP"].ToString().Substring(3);
                                }
                                cRTable2.Cells[0, 1].Text = sZIP.PadRight(8) + " " 
                                                          + Global.gcDataReader["ADDR1"].ToString()
                                                          + Global.gcDataReader["ADDR2"].ToString();
                                cRTable2.Cells[0, 1].Style.GridLines.Bottom = ld;

                                //住所等2行目
                                cRTable2.Cells[1, 1].Text = "TEL " + Global.gcDataReader["TEL"].ToString().PadRight(15) + "  "
                                                          + "FAX " + Global.gcDataReader["FAX"].ToString().PadRight(15);
                                cRTable2.Cells[1, 1].Style.GridLines.Bottom = ld;

                                //住所等3行目
                                cRTable2.Cells[2, 1].Text = "部署     " + StringCut(Global.gcDataReader["SBUSYO"].ToString().PadRight(30, ' '), 30) + "  "
                                                          + "相手先担当者 " + StringCut(Global.gcDataReader["STANTO"].ToString().PadRight(30, ' '), 30) + "  ";
                                                          //+ "敬称 " + Global.gcDataReader["KEISNM"].ToString().PadRight(3, '　');
                                cRTable2.Cells[2, 1].Style.GridLines.Bottom = ld;

                                //住所等4行目
                                cRTable2.Cells[3, 1].Text = "ﾒｰﾙｱﾄﾞﾚｽ " + Global.gcDataReader["TRMAIL"].ToString();
                                cRTable2.Cells[3, 1].Style.GridLines.Bottom = ld;

                                //住所等5行目
                                cRTable2.Cells[4, 1].Text = "ﾎｰﾑﾍﾟｰｼﾞ " + Global.gcDataReader["TRURL"].ToString();
                                cRTable2.Cells[4, 1].Style.GridLines.Bottom = ld;

                                //住所等6行目
                                cRTable2.Cells[5, 1].Text = "備考 " + StringCut(Global.gcDataReader["BIKO"].ToString().PadRight(60, ' '), 60) + "     " + "敬称 " + Global.gcDataReader["KEISNM"].ToString().PadRight(3, '　');
                                cRTable2.Cells[5, 1].Style.GridLines.Bottom = ld;

                                //住所等7行目
                                cRTable2.Cells[6, 1].Text = "自社営業担当者 " + Global.gcDataReader["TANTOMEI"].ToString();
                                cRTable2.Cells[6, 1].Style.GridLines.Bottom = ld;

                                //住所等8行目
                                cRTable2.Cells[7, 1].Text = "電子債権　利用者番号 " + Global.gcDataReader["CDM1"].ToString().PadRight(9, ' ') + "          " + "譲渡制限 " + (Global.gcDataReader["IDM1"].ToString() == "0" ? "しない" : "する");
                                cRTable2.Cells[7, 1].Style.GridLines.Bottom = ld;

                                //住所等9行目
                                cRTable2.Cells[8, 1].Text = "ﾏｲﾅﾝﾊﾞｰ　 法人番号 " + Global.gcDataReader["MYNO_AITE"].ToString().PadRight(13, ' ') + "        " + "相殺処理 許可" + (Global.gcDataReader["SOSAI"].ToString() == "0" ? "しない" : "する　") + "      " + (Global.gcDataReader["SRYOU_F"].ToString() == "0" ? "相殺領収書を発行しない" : "相殺領収書を発行する"); 

                                //string sKCOD = "";
                                //if (Global.gcDataReader["KCOD"].ToString() != "")
                                //{
                                //    if ((Global.nKCOD_ZE == 1) &&
                                //        (Global.nKCOD_Type == 0))
                                //    {
                                //        sKCOD = Convert.ToInt32(Global.gcDataReader["KCOD"].ToString()).ToString() + "：";
                                //    }
                                //    else
                                //    {
                                //        sKCOD = Global.gcDataReader["KCOD"].ToString() + "：";
                                //    }
                                //}
                                //if (sKCOD == "0")
                                //{
                                //    sKCOD = " ";
                                //    sKCOD = sKCOD.PadRight(Global.nKCOD_Len); //全科目は空白にする
                                //}
                                //cRTable2.Cells[3, 1].Text = "主担当 " + Global.gcDataReader["TNAM"].ToString().PadRight(7, '　') + "  "
                                //                          + "部門 " + Global.gcDataReader["BNAM"].ToString().PadRight(10, '　') + "  "
                                //                          + "科目 " + sKCOD + Global.gcDataReader["KNAM"].ToString().PadRight(11, '　');
                                nLineCnt = nLineCnt + 9;

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                //cRArea.Children.Add(cRTable2);
                                cC1PrnDoc.Body.Children.Add(cRTable2);
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                            }
                        }
                    }

//-- < 債権使用する>
                    // 回収設定がチェックされていた場合、出力
//                    if (Global.Prn_Kaisyu == 0)
                    if (Global.Prn_Kaisyu == 0 && Global.gcDataReader["SAIKEN"].ToString() == "1")
//-- <>
                    {
                        if (nLineCnt + 17 >= 62)
                        {
                            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                            //Sel_Header_Info(Global.sTRCDArray[i, 0], Global.sTRCDArray[i, 1]);
                            //if (Global.gcDataReader.HasRows == true)
                            //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();
                                //**
                                //**cRTable6.Width = "181.5mm";
                                //cRTable6.Width = "180mm";
                                //**

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                //cRArea.Children.Add(cRTable6);
                                cC1PrnDoc.Body.Children.Add(cRTable6);
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                //cRTable_H.BreakBefore = BreakEnum.Page;
                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[2, 1].Text = sWork;

                                //string sNAYOSE = (Global.gcDataReader["NAYOSE"].ToString() == "0" ? "×" : "○");
                                //string sSETUIN = (Global.gcDataReader["F_SETUIN"].ToString() == "0" ? "×" : "○");
                                //string sTGASW = (Global.gcDataReader["TGASW"].ToString() == "0" ? "                  " : "手形管理のみで使用");
                                //cRTable_H.Cells[1, 1].Text = Global.gcDataReader["RYAKU"].ToString().PadRight(10, '　').Substring(0, 10) + "    "
                                //                         + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "     "
                                //                         + "名寄せ" + sNAYOSE + "     " + "節印実行" + sSETUIN + "  "
                                //                         + sTGASW;
                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                //cRArea.Children.Add(cRTable_H);
                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応
                            }
                        }

                        // 回収設定部分
                        {
                            string sWork = "";
                            RenderTable cRTableK = new RenderTable();
                            for (int j = 0; j < 17; j++)
                            {
                                cRTableK.Rows[j].Height = "4mm";
                                cRTableK.Rows[j].Style.TextAlignVert = AlignVertEnum.Center;
                            }
                            cRTableK.Cols[0].Width = "20mm";
                            //**
                            //**cRTable2.Cols[1].Width = "161.5mm";
                            cRTableK.Cols[1].Width = "160mm";
                            //**
                            cRTableK.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTableK.Style.GridLines.All = fr;
                            cRTableK.Style.GridLines.Top = LineDef.Empty;
                            //回収設定タイトル
                            cRTableK.Cells[0, 0].SpanRows = 17;
                            cRTableK.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTableK.Cells[0, 0].Text = "回収設定";

                            //回収設定1行目
//-- <""> 
//                            cRTableK.Cells[0, 1].Text = "照合用ﾌﾘｶﾞﾅ　" + Global.gcDataReader["TOKUKANA"].ToString().PadRight(48, ' ') + "    [手数料負担] " + Get_Tesuu_NM(Convert.ToInt32(Global.gcDataReader["FUTAN"].ToString()));
                            cRTableK.Cells[0, 1].Text = "照合用ﾌﾘｶﾞﾅ　" + Global.gcDataReader["TOKUKANA"].ToString().PadRight(48, ' ') + "    [手数料負担] "
                                + Get_Tesuu_NM(Convert.ToInt32(Global.gcDataReader["FUTAN"].ToString() == "" ? "0" : Global.gcDataReader["FUTAN"].ToString()));
//-- <>
                            cRTableK.Cells[0, 1].Style.GridLines.Bottom = ld;

                            if (Global.gcDataReader["YAKUJO"].ToString() == "0")
                            {
                                //回収設定2行目
                                sWork = "回収方法　" + Global.gcDataReader["NYU_KBNMEI"].ToString();
                                //回収設定3行目
//-- <"">
//                                cRTableK.Cells[2, 1].Text = "　[締日] " + (Global.gcDataReader["SHIME"].ToString() == "99" ? "末" : Global.gcDataReader["SHIME"].ToString().PadLeft(2, ' ')) + "日"
//                                                          + "　[回収予定] " + Global.gcDataReader["KAISYUHI"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日"
//                                                          + "　[回収期日] " + Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日"
//                                                          + "　[休業日設定] " + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.gcDataReader["HOLIDAY"].ToString()));
                                switch (Global.gcDataReader["SHIME"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text = "　[締日]   日";
                                        break;
                                    case "99":
                                        cRTableK.Cells[2, 1].Text = "　[締日] 末日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text = "　[締日] " + (Global.gcDataReader["SHIME"].ToString() == "99" ? "末" : Global.gcDataReader["SHIME"].ToString().PadLeft(2, ' ')) + "日";
                                        break;
                                }
                                switch (Global.gcDataReader["KAISYUHI"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text += "　[回収予定]  ヶ月後   日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text += "　[回収予定] " + Global.gcDataReader["KAISYUHI"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                        break;
                                }

                                ////////// 期日ありのものに制限するのを入れないといけない
                                switch (Global.gcDataReader["KAISYUSIGHT"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text += "　[回収期日]  ヶ月後   日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text += "　[回収期日] " + Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                        break;
                                }
                                cRTableK.Cells[2, 1].Text += "　[休業日設定] " + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.gcDataReader["HOLIDAY"].ToString() == "" ? "0" : Global.gcDataReader["HOLIDAY"].ToString()));

//-- <>
                            }
                            else
                            {
                                sWork = "回収方法　約定";
                                //回収設定3行目
//-- <9999>
//                                cRTableK.Cells[2, 1].Text = "　[締日] " + (Global.gcDataReader["SHIME"].ToString() == "99" ? "末" : Global.gcDataReader["SHIME"].ToString().PadLeft(2, ' ')) + "日"
//                                                          + "　[回収予定] " + Global.gcDataReader["KAISYUHI"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日"
//                                                          + "　[回収期日] " + Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日"
//                                                          + "　[休業日設定] " + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.gcDataReader["HOLIDAY"].ToString()));
                                switch (Global.gcDataReader["SHIME"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text = "　[締日]   日";
                                        break;
                                    case "99":
                                        cRTableK.Cells[2, 1].Text = "　[締日] 末日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text = "　[締日] " + (Global.gcDataReader["SHIME"].ToString() == "99" ? "末" : Global.gcDataReader["SHIME"].ToString().PadLeft(2, ' ')) + "日";
                                        break;
                                }
                                switch (Global.gcDataReader["KAISYUHI"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text += "　[回収予定]  ヶ月後   日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text += "　[回収予定] " + Global.gcDataReader["KAISYUHI"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUHI"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                        break;
                                }

                                ////////// 期日ありのものに制限するのを入れないといけない
                                switch (Global.gcDataReader["KAISYUSIGHT"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[2, 1].Text += "　[回収期日]  ヶ月後   日";
                                        break;
                                    default:
                                        cRTableK.Cells[2, 1].Text += "　[回収期日] " + Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["KAISYUSIGHT"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                        break;
                                }
                                //回収設定4行目
//                                cRTableK.Cells[3, 1].Text = "　[約定金額]  " + Convert.ToDecimal(Global.gcDataReader["Y_KINGAKU"].ToString()).ToString("#,##0").PadLeft(15, ' ') + "円"
//                                                          + "　　　　 [約定金額未満]　" + Global.gcDataReader["MIMAN_NM"].ToString();
                                switch (Global.gcDataReader["Y_KINGAKU"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[3, 1].Text = "　[約定金額]                 円";
                                        break;
                                    default:
                                        cRTableK.Cells[3, 1].Text = "　[約定金額]  " + Convert.ToDecimal(Global.gcDataReader["Y_KINGAKU"].ToString()).ToString("#,##0").PadLeft(15, ' ') + "円";
                                        break;
                                }
                                switch (Global.gcDataReader["Y_KINGAKU"].ToString())
                                {
                                    default:
                                        cRTableK.Cells[3, 1].Text += "　　　　 [約定金額未満]　" + Global.gcDataReader["MIMAN_NM"].ToString();;
                                        break;
                                }

                                //回収設定5行目
//                                cRTableK.Cells[4, 1].Text = "　[約定金額以上①] " + StringCut(Global.gcDataReader["IJOU_NM1"].ToString().PadRight(10, '　'), 20) + "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_1"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%" + "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_1"].ToString())).PadRight(3, '　')
//                                                          + "　　　 " + "[回収期日] " + Global.gcDataReader["SIGHT_1"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_1"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_1"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                switch (Global.gcDataReader["IJOU_NM1"].ToString())
                                {
                                    default:
                                        cRTableK.Cells[4, 1].Text = "　[約定金額以上①] " + StringCut(Global.gcDataReader["IJOU_NM1"].ToString().PadRight(10, '　'), 20);
                                        break;
                                }
                                switch (Global.gcDataReader["BUNKATSU_1"].ToString())
                                {
                                    default:
                                        cRTableK.Cells[4, 1].Text += "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_1"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%";
                                        break;
                                }
                                switch (Global.gcDataReader["HASU_1"].ToString())
                                {
                                    default:
                                        cRTableK.Cells[4, 1].Text += "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_1"].ToString())).PadRight(3, '　');
                                        break;
                                }

                                ////////// 期日ありのものに制限するのを入れないといけない
                                switch (Global.gcDataReader["SIGHT_1"].ToString())
                                {
                                    case "":
                                    case "0":
                                        cRTableK.Cells[4, 1].Text += "　[回収期日]  ヶ月後   日";
                                        break;
                                    default:
                                        if (Global.gcDataReader["SIGHT_1"].ToString().Length != 3)
                                            {
                                                cRTableK.Cells[4, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_1"].ToString().PadLeft(3, '0').Substring(0, 1) + "ヶ月後 "
                                                    + (Global.gcDataReader["SIGHT_1"].ToString().PadLeft(3, '0').Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_1"].ToString().PadLeft(3, '0').Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                            else
                                            {
                                                cRTableK.Cells[4, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_1"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_1"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_1"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                        break;
                                }
                                //---> V01.18.01 HWPO UPDATE ▼(10174)
                                //if (Global.gcDataReader["IJOU_2"].ToString() != ""
                                if (Global.gcDataReader["IJOU_2"].ToString() != "" && Global.gcDataReader["IJOU_2"].ToString() != "0")
                                //<--- V01.18.01 HWPO UPDATE ▲(10174)
                                {
                                    //回収設定6行目
//                                    cRTableK.Cells[5, 1].Text = "　[約定金額以上②] " + StringCut(Global.gcDataReader["IJOU_NM2"].ToString().PadRight(10, '　'), 20) + "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_2"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%" + "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_2"].ToString())).PadRight(3, '　')
//                                                              + "　　　 " + "[回収期日] " + Global.gcDataReader["SIGHT_2"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_2"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_2"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                    switch (Global.gcDataReader["IJOU_NM2"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[5, 1].Text = "　[約定金額以上②] " + StringCut(Global.gcDataReader["IJOU_NM2"].ToString().PadRight(10, '　'), 20);
                                            break;
                                    }
                                    switch (Global.gcDataReader["BUNKATSU_2"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[5, 1].Text += "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_2"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%";
                                            break;
                                    }
                                    switch (Global.gcDataReader["HASU_2"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[5, 1].Text += "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_2"].ToString())).PadRight(3, '　');
                                            break;
                                    }

                                    ////////// 期日ありのものに制限するのを入れないといけない
                                    switch (Global.gcDataReader["SIGHT_2"].ToString())
                                    {
                                        case "":
                                        case "0":
                                            cRTableK.Cells[5, 1].Text += "　[回収期日]  ヶ月後   日";
                                            break;
                                        default:
                                            if (Global.gcDataReader["SIGHT_2"].ToString().Length != 3)
                                            {
                                                cRTableK.Cells[5, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_2"].ToString().PadLeft(3, '0').Substring(0, 1) + "ヶ月後 "
                                                    + (Global.gcDataReader["SIGHT_2"].ToString().PadLeft(3, '0').Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_2"].ToString().PadLeft(3, '0').Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                            else
                                            {
                                                cRTableK.Cells[5, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_2"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_2"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_2"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                            break;
                                    }
                                }

                                //---> V02.01.01 HWPO UPDATE ▼(10174)
                                //if (Global.gcDataReader["IJOU_3"].ToString() != "")
                                if (Global.gcDataReader["IJOU_3"].ToString() != "" && Global.gcDataReader["IJOU_3"].ToString() != "0")
                                //<--- V02.01.01 HWPO UPDATE ▲(10174)
                                {
                                    //回収設定7行目
//                                    cRTableK.Cells[6, 1].Text = "　[約定金額以上③] " + StringCut(Global.gcDataReader["IJOU_NM3"].ToString().PadRight(10, '　'), 20) + "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_3"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%" + "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_3"].ToString())).PadRight(3, '　')
//                                                              + "　　　 " + "[回収期日] " + Global.gcDataReader["SIGHT_3"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_3"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_3"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                    switch (Global.gcDataReader["IJOU_NM3"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[6, 1].Text = "　[約定金額以上③] " + StringCut(Global.gcDataReader["IJOU_NM3"].ToString().PadRight(10, '　'), 20);
                                            break;
                                    }
                                    switch (Global.gcDataReader["BUNKATSU_3"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[6, 1].Text += "  " + Convert.ToDecimal(Global.gcDataReader["BUNKATSU_3"].ToString()).ToString("#0.0").PadLeft(4, ' ') + "%";
                                            break;
                                    }
                                    switch (Global.gcDataReader["HASU_3"].ToString())
                                    {
                                        default:
                                            cRTableK.Cells[6, 1].Text += "      " + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.gcDataReader["HASU_3"].ToString())).PadRight(3, '　');
                                            break;
                                    }

                                    ////////// 期日ありのものに制限するのを入れないといけない
                                    switch (Global.gcDataReader["SIGHT_3"].ToString())
                                    {
                                        case "":
                                        case "0":
                                            cRTableK.Cells[6, 1].Text += "　[回収期日]  ヶ月後   日";
                                            break;
                                        default:
                                            if (Global.gcDataReader["SIGHT_3"].ToString().Length != 3)
                                            {
                                                cRTableK.Cells[6, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_3"].ToString().PadLeft(3, '0').Substring(0, 1) + "ヶ月後 " 
                                                    + (Global.gcDataReader["SIGHT_3"].ToString().PadLeft(3, '0').Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_3"].ToString().PadLeft(3, '0').Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                            else
                                            {
                                                cRTableK.Cells[6, 1].Text += "　[回収期日] " + Global.gcDataReader["SIGHT_3"].ToString().Substring(0, 1) + "ヶ月後 " + (Global.gcDataReader["SIGHT_3"].ToString().Substring(1, 2) == "99" ? "末" : Global.gcDataReader["SIGHT_3"].ToString().Substring(1, 2).PadLeft(2, ' ')) + "日";
                                            }
                                            break;
                                    }
                                }
//-- <2016/03/12 休業日を入れ忘れ>
                                cRTableK.Cells[2, 1].Text += "　[休業日設定] " + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.gcDataReader["HOLIDAY"].ToString() == "" ? "0" : Global.gcDataReader["HOLIDAY"].ToString()));
//-- <2016/03/12>                                
//-- <9999>
                            }
                            cRTableK.Cells[1, 1].Text = sWork;
                            cRTableK.Cells[1, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[2, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[3, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[4, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[5, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[6, 1].Style.GridLines.Bottom = ld;

                            if (Global.gcDataReader["GAIKA"].ToString() == "0")
                            {
                                //回収設定8行目
                                cRTableK.Cells[7, 1].Text = "外貨を使用しない" + " ".PadRight(22, ' ') + "取引通貨　";
                            }
                            else
                            {
                                //回収設定8行目
                                cRTableK.Cells[7, 1].Text = "外貨を使用する　" + " ".PadRight(22, ' ') + "取引通貨　" + Global.gcDataReader["TSUKA"].ToString();
                                //回収設定9行目
                                cRTableK.Cells[8, 1].Text = "　照合ｷｰ(前)　" + Global.gcDataReader["GAIKA_KEY_F"].ToString();
                                //回収設定10行目
                                cRTableK.Cells[9, 1].Text = "　照合ｷｰ(後)　" + Global.gcDataReader["GAIKA_KEY_B"].ToString();
                            }
                            cRTableK.Cells[7, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[8, 1].Style.GridLines.Bottom = ld;
                            cRTableK.Cells[9, 1].Style.GridLines.Bottom = ld;

                            //回収設定11行目
                            sWork = "被振込口座①　  ";
                            if (Global.gcDataReader["HIFURIKOZA_1"].ToString() != "")
                            {
//-- <2016/03/12 口座IDは8ﾊﾞｲﾄ　後ろの文字列は名称を20ﾊﾞｲﾄ分固定にする>
//                                sWork += Global.gcDataReader["HIFURIKOZA_1"].ToString().PadLeft(10, ' ') + " " + Global.gcDataReader["HI_BKCOD1"].ToString() + " "
//                                       + Global.gcDataReader["HI_BKNAM1"].ToString().PadRight(10, '　') + " " + Global.gcDataReader["HI_BRCOD1"].ToString() + " "
//                                       + Global.gcDataReader["HI_BRNAM1"].ToString().PadRight(10, '　') + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND1"].ToString()) + " "
//                                       + Global.gcDataReader["HI_KOZANO1"].ToString();
                                sWork += Global.gcDataReader["HIFURIKOZA_1"].ToString().PadLeft(8, ' ') + " " + Global.gcDataReader["HI_BKCOD1"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BKNAM1"].ToString()), 20) + " " + Global.gcDataReader["HI_BRCOD1"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BRNAM1"].ToString()), 20) + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND1"].ToString()) + " "
                                       + Global.gcDataReader["HI_KOZANO1"].ToString();
//-- <2016/03/12>
                            }
                            cRTableK.Cells[10, 1].Text = sWork;
                            cRTableK.Cells[10, 1].Style.GridLines.Bottom = ld;

                            //回収設定12行目
                            sWork = "被振込口座②　  ";
                            if (Global.gcDataReader["HIFURIKOZA_2"].ToString() != "")
                            {
//-- <2016/03/12>
//                                sWork += Global.gcDataReader["HIFURIKOZA_2"].ToString().PadLeft(10, ' ') + " " + Global.gcDataReader["HI_BKCOD2"].ToString() + " "
//                                       + Global.gcDataReader["HI_BKNAM2"].ToString().PadRight(10, '　') + " " + Global.gcDataReader["HI_BRCOD2"].ToString() + " "
//                                       + Global.gcDataReader["HI_BRNAM2"].ToString().PadRight(10, '　') + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND2"].ToString()) + " "
//                                       + Global.gcDataReader["HI_KOZANO2"].ToString();
                                sWork += Global.gcDataReader["HIFURIKOZA_2"].ToString().PadLeft(8, ' ') + " " + Global.gcDataReader["HI_BKCOD2"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BKNAM2"].ToString()), 20) + " " + Global.gcDataReader["HI_BRCOD2"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BRNAM2"].ToString()), 20) + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND2"].ToString()) + " "
                                       + Global.gcDataReader["HI_KOZANO2"].ToString();
//-- <2016/03/12>
                            }
                            cRTableK.Cells[11, 1].Text = sWork;
                            cRTableK.Cells[11, 1].Style.GridLines.Bottom = ld;

                            //回収設定13行目
                            sWork = "被振込口座③　  ";
                            if (Global.gcDataReader["HIFURIKOZA_3"].ToString() != "")
                            {
//-- <2016/03/12>
//                                sWork += Global.gcDataReader["HIFURIKOZA_3"].ToString().PadLeft(10, ' ') + " " + Global.gcDataReader["HI_BKCOD3"].ToString() + " "
//                                       + Global.gcDataReader["HI_BKNAM3"].ToString().PadRight(10, '　') + " " + Global.gcDataReader["HI_BRCOD3"].ToString() + " "
//                                       + Global.gcDataReader["HI_BRNAM3"].ToString().PadRight(10, '　') + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND3"].ToString()) + " "
//                                       + Global.gcDataReader["HI_KOZANO3"].ToString();
                                sWork += Global.gcDataReader["HIFURIKOZA_3"].ToString().PadLeft(8, ' ') + " " + Global.gcDataReader["HI_BKCOD3"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BKNAM3"].ToString()), 20) + " " + Global.gcDataReader["HI_BRCOD3"].ToString() + " "
                                       + StringCut(string.Format("{0,-20}", Global.gcDataReader["HI_BRNAM3"].ToString()), 20) + " " + Get_YokinType_NM(Global.gcDataReader["HI_YOKINKIND3"].ToString()) + " "
                                       + Global.gcDataReader["HI_KOZANO3"].ToString();
//-- <2016/03/12>
                            }
                            cRTableK.Cells[12, 1].Text = sWork;
                            cRTableK.Cells[12, 1].Style.GridLines.Bottom = ld;

                            //回収設定14行目
                            sWork = "";
                            string wk1 = "";
                            string wk2 = "";
                            string wk3 = "";
                            switch (Global.gcDataReader["YOKINSYU"].ToString())
                            {
                                case "1":
                                    wk3 = "普通";
                                    break;
                                case "2":
                                    wk3 = "当座";
                                    break;
                                case "4":
                                    wk3 = "貯蓄";
                                    break;
                                case "5":
                                    wk3 = "通知";
                                    break;
                                default:
                                    wk3 = "　　";
                                    break;
                            }
                            if (Global.gcDataReader["SEN_KOZANO"].ToString().Length == 10)
                            {
                                wk1 = Global.gcDataReader["SEN_KOZANO"].ToString().Substring(0, 3);
                                wk2 = Global.gcDataReader["SEN_KOZANO"].ToString().Substring(3, 7);
                                sWork = wk1 + " " + mcBsLogic.StringCut(Global.gcDataReader["SEN_SHITENMEI"].ToString().PadRight(10, '　'), 20) + " " + wk3 + " " + wk2;
                            }
                            else if (Global.gcDataReader["SEN_KOZANO"].ToString().Length == 7)
                            {
                                wk1 = "   ";
                                wk2 = Global.gcDataReader["SEN_KOZANO"].ToString();
                                sWork = wk1 + " " + mcBsLogic.StringCut(Global.gcDataReader["SEN_SHITENMEI"].ToString().PadRight(10, '　'), 20) + " " + wk3 + " " + wk2;
                            }
                            else if (Global.gcDataReader["SEN_KOZANO"].ToString().Length == 3)
                            {
                                wk1 = Global.gcDataReader["SEN_KOZANO"].ToString();
                                wk2 = "       ";
                                sWork = wk1 + " " + mcBsLogic.StringCut(Global.gcDataReader["SEN_SHITENMEI"].ToString().PadRight(10, '　'), 20) + " " + wk3 + " " + wk2;
                            }
                            else
                            {
                                wk1 = "   ";
                                wk2 = "       ";
                                sWork = wk1 + " " + mcBsLogic.StringCut(Global.gcDataReader["SEN_SHITENMEI"].ToString().PadRight(10, '　'), 20) + " " + wk3 + " " + wk2;
                            }
                            cRTableK.Cells[13, 1].Text = "専用入金口座　" + Global.gcDataReader["SEN_GINKOCD"].ToString() + " " + Global.gcDataReader["SEN_BKNAM"].ToString().PadRight(10, '　') + " " + Global.gcDataReader["SEN_SITENCD"].ToString() + " 仮想支店 " + sWork;
                            cRTableK.Cells[13, 1].Style.GridLines.Bottom = ld;

                            //回収設定15行目
                            wk1 = "カナ自動学習　  " + (Global.gcDataReader["JIDOU_GAKUSYU"].ToString() == "0" ? "しない" : "する　");
                            wk2 = "入金予定利用　  " + (Global.gcDataReader["NYUKIN_YOTEI"].ToString() == "0" ? "しない" : "する　");
                            wk3 = "領収書発行　  " + (Global.gcDataReader["RYOSYUSYO"].ToString() == "0" ? "しない" : "する");
                            sWork = wk1 + "　　　　　" + wk2 + "　　　　　" + wk3;
                            cRTableK.Cells[14, 1].Text = sWork;
                            cRTableK.Cells[14, 1].Style.GridLines.Bottom = ld;

                            //回収設定16行目
                            // ---> V02.40.01 YMP UPDATE ▼(127389)
                            //wk1 = "手数料自動学習　" + (Global.gcDataReader["JIDOU_GAKUSYU"].ToString() == "0" ? "しない" : "する　");
                            //wk2 = "手数料誤差利用　" + (Global.gcDataReader["NYUKIN_YOTEI"].ToString() == "0" ? "しない" : "する　");
                            wk1 = "手数料自動学習　" + (Global.gcDataReader["TESURYO_GAKUSYU"].ToString() == "0" ? "しない" : "する　");
                            wk2 = "手数料誤差利用　" + (Global.gcDataReader["TESURYO_GOSA"].ToString() == "0" ? "しない" : "する　");
                            // <--- V02.40.01 YMP UPDATE ▲(127389)
                            sWork = wk1 + "　　　　　" + wk2;
                            cRTableK.Cells[15, 1].Text = sWork;
                            cRTableK.Cells[15, 1].Style.GridLines.Bottom = ld;

                            //回収設定17行目
                            wk1 = "信用調査用企業ｺｰﾄﾞ　" + Global.gcDataReader["SHIN_KAISYACD"].ToString().PadRight(15, ' ');
                            //---> V01.18.01 HWPO ADD ▼(10174)
                            if (string.IsNullOrEmpty(Global.gcDataReader["YOSIN"].ToString()))
                            {
                                wk2 = "与信限度額　" + "0".PadLeft(15, ' ') + "円";
                            }                            
                            else
                            {
                            //<--- V01.18.01 HWPO ADD ▲(10174)
                                wk2 = "与信限度額　" + Convert.ToDecimal(Global.gcDataReader["YOSIN"].ToString()).ToString("#,##0").PadLeft(15, ' ') + "円";
                            }                            
                            wk3 = "与信ﾗﾝｸ　" + Global.gcDataReader["YOSHINRANK"].ToString();
                            sWork = wk1 + "　　　 " + wk2 + "　　　 " + wk3;
                            cRTableK.Cells[16, 1].Text = sWork;
                            cRTableK.Cells[16, 1].Style.GridLines.Bottom = fr;

                            nLineCnt = nLineCnt + 17;

                            cC1PrnDoc.Body.Children.Add(cRTableK);
                        }

                    }

                    //支払方法がチェックされていた場合、出力
//                    if (Global.Prn_Shiharai == 0)
                    if (Global.Prn_Shiharai == 0 && Global.gcDataReader["SAIMU"].ToString() == "1")
//--
                    {
                        int nCnt = Sel_Grp3_Info_Pre(Global.gcDataReader["TRCD"].ToString(), Global.gcDataReader["HJCD"].ToString());

                        if (nCnt > 0 && nLineCnt + 11 >= 62)     //←★処理中ページに１データも入らなければ改ページ処理するよう修正。
                        {
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();

                                cC1PrnDoc.Body.Children.Add(cRTable6);

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[2, 1].Text = sWork;

                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                            }
                        }
                        else
                        {
                            //改頁前の下線補完
                            RenderTable cRTable6 = new RenderTable();
                            cC1PrnDoc.Body.Children.Add(cRTable6);
                        }

                        if (nCnt > 0)
                        {
                            int iRoopCnt = nCnt;

                            RenderTable cRTable4 = new RenderTable();
                            cRTable4.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable4.Cols[0].Width = "20mm";
                            cRTable4.Cols[1].Width = "3.5mm";
                            cRTable4.Cols[2].Width = "35mm";
                            cRTable4.Cols[3].Width = "45mm";
                            cRTable4.Cols[4].Width = "25mm";
                            cRTable4.Cols[5].Width = "15mm";
                            cRTable4.Cols[6].Width = "10mm";
                            cRTable4.Cols[7].Width = "26.5mm"; //31.5mm
                            cRTable4.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTable4.Style.GridLines.All = fr;
                            cRTable4.Style.GridLines.Top = LineDef.Empty;
                            //支払方法タイトル
                            cRTable4.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;

                            cRTable4.Cells[0, 0].Text = "支払条件";

                            //支払方法タブのデータを取得
                            {
                                int n = 0;

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                int nSubDataIdx = 0;
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                                //while (Global.gcDataReader.Read())
                                while (nSubDataIdx < nCnt)
                                //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                                {
                                    n++;
                                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                    nSubDataIdx++;
                                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                    //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                                    if (nLineCnt + 11 >= 62)
                                    {
                                        if (n > 1)  //←保険。n=1では来ないはずだが・・・
                                        {
                                            cRTable4.Cells[0, 0].SpanRows = 11 * (n - 1);
                                            cRTable4.Cells[(n - 1) * 11 - 1, 1].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 2].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 3].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 4].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 5].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 6].Style.GridLines.Bottom = fr;
                                            cRTable4.Cells[(n - 1) * 11 - 1, 7].Style.GridLines.Bottom = fr;
                                        }

                                        nLineCnt = 0;

                                        cC1PrnDoc.Body.Children.Add(cRTable4);
                                        RenderTable cRTableH = new RenderTable();
                                        cRTableH = (RenderTable)cRTableFuri.Clone();
                                        PrintDivisionResult eRet = PrintDivisionTrans();
                                        switch (eRet)
                                        {
                                            case PrintDivisionResult.NoDivision:            //
                                            case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                            case PrintDivisionResult.Unreached:             //
                                            case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                                break;
                                            case PrintDivisionResult.Preview:
                                            case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                            case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                            case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                                return null;
                                        }
                                        if (cC1PrnDoc.Body.Children.Count > 0)
                                        {
                                            cRTableH.BreakBefore = BreakEnum.Page;
                                        }
                                        cC1PrnDoc.Body.Children.Add(cRTableH);
                                        nLineCnt = 3;
                                        cRTable4 = new RenderTable();
                                        cRTable4.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                        cRTable4.Cols[0].Width = "20mm";
                                        cRTable4.Cols[1].Width = "3.5mm";
                                        cRTable4.Cols[2].Width = "35mm";
                                        cRTable4.Cols[3].Width = "45mm";
                                        cRTable4.Cols[4].Width = "25mm";
                                        cRTable4.Cols[5].Width = "15mm";
                                        cRTable4.Cols[6].Width = "10mm";
                                        cRTable4.Cols[7].Width = "26.5mm";
                                        cRTable4.Cols[1].CellStyle.Spacing.Left = "1mm";
                                        cRTable4.Style.GridLines.All = fr;
                                        cRTable4.Style.GridLines.Top = LineDef.Empty;
                                        //支払方法タイトル
                                        cRTable4.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                                        cRTable4.Cells[0, 0].Text = "支払条件";
                                        n = 1;
                                    }

                                    for (int ii = (n - 1) * 11; ii < (n - 1) * 11 + 11; ii++)
                                    {
                                        cRTable4.Rows[ii].Height = "4mm";
                                    }

                                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応

                                    cRTable4.Cells[(n - 1) * 11, 1].Text = "ID:"
                                                                        + Global.drTSHOH_SJ[nSubDataIdx - 1]["SHO_ID"].ToString().PadLeft(2, '0') + "  ";

                                    //支払方法1行目
                                    string sBCOD = "0";
                                    string sKCOD = "0";
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["BCOD"].ToString() != "")
                                    {
                                        if ((Global.nBCOD_ZE == 1) &&
                                            (Global.nBCOD_Type == 0))
                                        {
                                            sBCOD = Convert.ToInt32(Global.drTSHOH_SJ[nSubDataIdx - 1]["BCOD"].ToString()).ToString();
                                        }
                                        else
                                        {
                                            sBCOD = Global.drTSHOH_SJ[nSubDataIdx - 1]["BCOD"].ToString();
                                        }
                                    }
                                    if (sBCOD != "0")
                                    {
                                        sBCOD = sBCOD.PadRight(Global.nBCOD_Len) + " "
                                              + Global.drTSHOH_SJ[nSubDataIdx - 1]["BNAM"].ToString().PadRight(10, '　');
                                    }
                                    else
                                    {
                                        sBCOD = "".PadRight(Global.nBCOD_Len) + " 全て".PadRight(10, '　');
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["KCOD"].ToString() != "")
                                    {
                                        if ((Global.nKCOD_ZE == 1) &&
                                            (Global.nKCOD_Type == 0))
                                        {
                                            sKCOD = Convert.ToInt32(Global.drTSHOH_SJ[nSubDataIdx - 1]["KCOD"].ToString()).ToString();
                                        }
                                        else
                                        {
                                            sKCOD = Global.drTSHOH_SJ[nSubDataIdx - 1]["KCOD"].ToString();
                                        }
                                    }
                                    if (sKCOD != "0")
                                    {
                                        sKCOD = sKCOD.PadRight(Global.nKCOD_Len) + " "
                                              + Global.drTSHOH_SJ[nSubDataIdx - 1]["KNAM"].ToString().PadRight(11, '　');
                                    }
                                    else
                                    {
                                        sKCOD = "".PadRight(Global.nKCOD_Len) + " 全て".PadRight(10, '　');
                                    }
                                    cRTable4.Cells[(n - 1) * 11, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11, 1].SpanCols = 7; //5
                                    cRTable4.Cells[(n - 1) * 11, 1].Text += "[対象部門] " + sBCOD + "　　[対象科目] " + sKCOD;

                                    //支払方法2行目
                                    cRTable4.Cells[(n - 1) * 11 + 1, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 1, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 1, 1].SpanCols = 7;　//5
                                    cRTable4.Cells[(n - 1) * 11 + 1, 1].Text = "　[支払方法] " + Global.drTSHOH_SJ[nSubDataIdx - 1]["SHINO"].ToString().PadLeft(3, '0') + " "
                                                              + Global.drTSHOH_SJ[nSubDataIdx - 1]["SICOMENT"].ToString().PadRight(30, '　');
                                    //支払方法3行目
                                    cRTable4.Cells[(n - 1) * 11 + 2, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 2, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 2, 1].SpanCols = 7; //5
                                    string sSIMEBI = "";
                                    string sSIHARAIDD = "";
                                    string sSKIJITUDD = "";
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SIMEBI"].ToString() == "99")
                                    {
                                        sSIMEBI = "末";
                                    }
                                    else
                                    {
                                        sSIMEBI = Global.drTSHOH_SJ[nSubDataIdx - 1]["SIMEBI"].ToString().PadLeft(2);
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SIHARAIDD"].ToString() == "99")
                                    {
                                        sSIHARAIDD = "末";
                                    }
                                    else
                                    {
                                        sSIHARAIDD = Global.drTSHOH_SJ[nSubDataIdx - 1]["SIHARAIDD"].ToString().PadLeft(2);
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKIJITUDD"].ToString() == "99")
                                    {
                                        sSKIJITUDD = "末";
                                    }
                                    else
                                    {
                                        sSKIJITUDD = Global.drTSHOH_SJ[nSubDataIdx - 1]["SKIJITUDD"].ToString().PadLeft(2);
                                    }
                                    string sHARAI_H = "";
                                    string sKIJITU_H = "";
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["HARAI_H"].ToString() == "0")
                                    {
                                        sHARAI_H = "前営業日";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["HARAI_H"].ToString() == "1")
                                    {
                                        sHARAI_H = "当日";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["HARAI_H"].ToString() == "2")
                                    {
                                        sHARAI_H = "後営業日";
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["KIJITU_H"].ToString() == "0")
                                    {
                                        sKIJITU_H = "前営業日";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["KIJITU_H"].ToString() == "1")
                                    {
                                        sKIJITU_H = "当日";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["KIJITU_H"].ToString() == "2")
                                    {
                                        sKIJITU_H = "後営業日";
                                    }
                                    cRTable4.Cells[(n - 1) * 11 + 2, 1].Text = "　[締日] " + sSIMEBI + "日" + "  "
                                                              + "[支払日] " + Global.drTSHOH_SJ[nSubDataIdx - 1]["SHIHARAIMM"].ToString().PadLeft(2) + "ヶ月後 "
                                                              + sSIHARAIDD + "日 (" + sHARAI_H.PadRight(4, '　')
                                                              + ") [支払期日] " + Global.drTSHOH_SJ[nSubDataIdx - 1]["SKIJITUMM"].ToString().PadLeft(2) + "ヶ月後 "
                                                              + sSKIJITUDD + "日 (" + sKIJITU_H.PadRight(4, '　') + ")";
                                    //支払方法4行目
                                    cRTable4.Cells[(n - 1) * 11 + 3, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 3, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 3, 1].SpanCols = 2;
                                    cRTable4.Cells[(n - 1) * 11 + 3, 1].SpanCols = 7;
                                    cRTable4.Cells[(n - 1) * 11 + 3, 1].Text = "　[支払区分] " + Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM"].ToString().PadRight(18, '　');
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM"].ToString() == "約定A")
                                    {
                                        cRTable4.Cells[(n - 1) * 11 + 3, 1].Text += " [約定金額]  " + Convert.ToDecimal(Global.drTSHOH_SJ[nSubDataIdx - 1]["V_YAKUJO"].ToString()).ToString("#,#0").PadLeft(19, ' ') + "円";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM"].ToString() == "約定B")
                                    {
                                        cRTable4.Cells[(n - 1) * 11 + 3, 1].Text += " [約定金額]  " + Convert.ToDecimal(Global.drTSHOH_SJ[nSubDataIdx - 1]["V_YAKUJO"].ToString()).ToString("#,#0").PadLeft(19, ' ') + "円";
                                    }
                                    else
                                    {
                                        cRTable4.Cells[(n - 1) * 11 + 3, 4].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 3, 4].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 3, 4].SpanCols = 4;
                                        cRTable4.Cells[(n - 1) * 11 + 3, 4].Text += "";
                                    }
                                    string sYAKUJOB_U1 = "";
                                    string sYAKUJOB_U2 = "";
                                    string sYAKUJOB_U3 = "";
                                    //string sYAKUJOB_S1 = "";
                                    //string sYAKUJOB_S2 = "";
                                    //string sYAKUJOB_S3 = "";
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "6")
                                    {
                                        sYAKUJOB_U1 = "十万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "5")
                                    {
                                        sYAKUJOB_U1 = "万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "4")
                                    {
                                        sYAKUJOB_U1 = "千";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "3")
                                    {
                                        sYAKUJOB_U1 = "百";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "2")
                                    {
                                        sYAKUJOB_U1 = "十";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "1")
                                    {
                                        sYAKUJOB_U1 = "一";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U1"].ToString() == "0")
                                    {
                                        sYAKUJOB_U1 = "端数";
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "6")
                                    {
                                        sYAKUJOB_U2 = "十万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "5")
                                    {
                                        sYAKUJOB_U2 = "万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "4")
                                    {
                                        sYAKUJOB_U2 = "千";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "3")
                                    {
                                        sYAKUJOB_U2 = "百";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "2")
                                    {
                                        sYAKUJOB_U2 = "十";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "1")
                                    {
                                        sYAKUJOB_U2 = "一";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U2"].ToString() == "0")
                                    {
                                        sYAKUJOB_U2 = "端数";
                                    }
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "6")
                                    {
                                        sYAKUJOB_U3 = "十万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "5")
                                    {
                                        sYAKUJOB_U3 = "万";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "4")
                                    {
                                        sYAKUJOB_U3 = "千";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "3")
                                    {
                                        sYAKUJOB_U3 = "百";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "2")
                                    {
                                        sYAKUJOB_U3 = "十";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "1")
                                    {
                                        sYAKUJOB_U3 = "一";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_U3"].ToString() == "0")
                                    {
                                        sYAKUJOB_U3 = "端数";
                                    }

                                    //if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S1"].ToString() == "1")
                                    //{
                                    //    sYAKUJOB_S1 = "切り捨て";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S1"].ToString() == "2")
                                    //{
                                    //    sYAKUJOB_S1 = "四捨五入";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S1"].ToString() == "3")
                                    //{
                                    //    sYAKUJOB_S1 = "切り上げ";
                                    //}
                                    //if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S2"].ToString() == "1")
                                    //{
                                    //    sYAKUJOB_S2 = "切り捨て";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S2"].ToString() == "2")
                                    //{
                                    //    sYAKUJOB_S2 = "四捨五入";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S2"].ToString() == "3")
                                    //{
                                    //    sYAKUJOB_S2 = "切り上げ";
                                    //}
                                    //if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S3"].ToString() == "1")
                                    //{
                                    //    sYAKUJOB_S3 = "切り捨て";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S3"].ToString() == "2")
                                    //{
                                    //    sYAKUJOB_S3 = "四捨五入";
                                    //}
                                    //else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_S3"].ToString() == "3")
                                    //{
                                    //    sYAKUJOB_S3 = "切り上げ";
                                    //}
                                    //約定A・約定B・その他で出力内容に差分有り
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM"].ToString() == "約定A")
                                    {
                                        //支払方法5行目
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Text = "　[約定Ａ 以下額の支払] "
                                                                    + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOA_L"]).PadRight(5, '　') + "[超過額の支払] "
                                                                    + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOA_M"]).PadRight(5, '　');
                                        //支払方法6行目
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Text = " ";
                                        //支払方法7行目
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Text = " ";
                                    }
                                    else if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM"].ToString() == "約定B")
                                    {
                                        //支払方法5行目
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 2].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 2].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 3].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 3].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 4].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 4].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 5].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 5].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 6].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 6].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 7].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 7].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].SpanCols = 3;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Text = "　[約定Ｂ 以下額] "
                                                                                + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_LH"]).PadRight(10, '　') + "[超過額]";
                                        cRTable4.Cells[(n - 1) * 11 + 4, 4].Text = "① " + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_H1"]).PadRight(5, '　');
                                        cRTable4.Cells[(n - 1) * 11 + 4, 5].Text = Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_R1"].ToString().PadLeft(5) + "% ";
                                        cRTable4.Cells[(n - 1) * 11 + 4, 6].Text = sYAKUJOB_U1.PadRight(2, '　');
                                        //cRTable4.Cells[(n - 1) * 12 + 4, 7].Text = sYAKUJOB_S1.PadRight(4, '　');
                                        //支払方法6行目
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 2].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 2].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 3].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 3].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 4].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 4].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 5].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 5].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 6].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 6].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 7].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 7].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 4].Text = "② " + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_H2"]).PadRight(5, '　');
                                        cRTable4.Cells[(n - 1) * 11 + 5, 5].Text = Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_R2"].ToString().PadLeft(5) + "% ";
                                        cRTable4.Cells[(n - 1) * 11 + 5, 6].Text = sYAKUJOB_U2.PadRight(2, '　');
                                        //cRTable4.Cells[(n - 1) * 12 + 5, 7].Text = sYAKUJOB_S2.PadRight(4, '　');
                                        //支払方法7行目
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 2].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 2].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 3].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 3].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 4].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 4].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 5].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 5].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 6].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 6].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 7].Style.GridLines.Left = LineDef.Empty;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 7].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 4].Text = "③ " + GetSKBNM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_H3"]).PadRight(5, '　');
                                        cRTable4.Cells[(n - 1) * 11 + 6, 5].Text = Global.drTSHOH_SJ[nSubDataIdx - 1]["YAKUJOB_R3"].ToString().PadLeft(5) + "% ";
                                        cRTable4.Cells[(n - 1) * 11 + 6, 6].Text = sYAKUJOB_U3.PadRight(2, '　');
                                        //cRTable4.Cells[(n - 1) * 12 + 6, 7].Text = sYAKUJOB_S3.PadRight(4, '　');
                                    }
                                    else
                                    {
                                        //約定A・B以外は印字無し
                                        //支払方法5行目
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 4, 1].Text = " ";
                                        //支払方法6行目
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 5, 1].Text = " ";
                                        //支払方法7行目
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Left = fr;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Style.GridLines.Bottom = ld;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].SpanCols = 7;
                                        cRTable4.Cells[(n - 1) * 11 + 6, 1].Text = " ";
                                    }

                                    //支払方法8行目
                                    cRTable4.Cells[(n - 1) * 11 + 7, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 7, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 7, 1].SpanCols = 7;
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM1"].ToString() != "")
                                    {
                                        if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBKIND1"].ToString() != "8")
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 7, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM1"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBKCOD1"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BKNAM1"].ToString().PadRight(10, '　'), 20) + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBRCOD1"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BRNAM1"].ToString().PadRight(10, '　'), 20) + " " + Get_YokinType_NM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YOKNKIND1"].ToString())
                                                                                     + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["KOZANO1"].ToString();
                                        }
                                        else
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 7, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM1"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["FACNAM1"].ToString();
                                        }
                                    }
                                    //支払方法9行目
                                    cRTable4.Cells[(n - 1) * 11 + 8, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 8, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 8, 1].SpanCols = 7;
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM2"].ToString() != "")
                                    {
                                        if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBKIND2"].ToString() != "8")
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 8, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM2"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBKCOD2"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BKNAM2"].ToString().PadRight(10, '　'), 20) + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBRCOD2"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BRNAM2"].ToString().PadRight(10, '　'), 20) + " " + Get_YokinType_NM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YOKNKIND2"].ToString())
                                                                                     + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["KOZANO2"].ToString();
                                        }
                                        else
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 8, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM2"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["FACNAM2"].ToString();
                                        }
                                    }
                                    //支払方法10行目
                                    cRTable4.Cells[(n - 1) * 11 + 9, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 9, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 9, 1].SpanCols = 7;
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM3"].ToString() != "")
                                    {
                                        if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBKIND3"].ToString() != "8")
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 9, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM3"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBKCOD3"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BKNAM3"].ToString().PadRight(10, '　'), 20) + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBRCOD3"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BRNAM3"].ToString().PadRight(10, '　'), 20) + " " + Get_YokinType_NM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YOKNKIND3"].ToString())
                                                                                     + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["KOZANO3"].ToString();
                                        }
                                        else
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 9, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM3"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["FACNAM3"].ToString();
                                        }
                                    }
                                    //支払方法11行目
                                    cRTable4.Cells[(n - 1) * 11 + 10, 1].Style.GridLines.Left = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 1].Style.GridLines.Bottom = ld;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 1].SpanCols = 7;
                                    if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM4"].ToString() != "")
                                    {
                                        if (Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBKIND4"].ToString() != "8")
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 10, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM4"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBKCOD4"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BKNAM4"].ToString().PadRight(10, '　'), 20) + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["OWNBRCOD4"].ToString()
                                                                                     + " " + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["BRNAM4"].ToString().PadRight(10, '　'), 20) + " " + Get_YokinType_NM(Global.drTSHOH_SJ[nSubDataIdx - 1]["YOKNKIND4"].ToString())
                                                                                     + " " + Global.drTSHOH_SJ[nSubDataIdx - 1]["KOZANO4"].ToString();
                                        }
                                        else
                                        {
                                            cRTable4.Cells[(n - 1) * 11 + 10, 1].Text = "　" + mcBsLogic.StringCut(Global.drTSHOH_SJ[nSubDataIdx - 1]["SKBNM4"].ToString().PadRight(10, '　'), 20) + "　　 " + Global.drTSHOH_SJ[nSubDataIdx - 1]["FACNAM4"].ToString();
                                        }
                                    }

                                    nLineCnt = nLineCnt + 11;

                                    //2013/07/16 ICS.居軒 ▲パフォーマンス改善

                                }

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                cRTable4.Cells[0, 0].SpanRows = 12 * n;
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                                if (cRTable4.Cells[(n - 1) * 11 + 10, 1].SpanCols == 7)
                                {
                                    cRTable4.Cells[(n - 1) * 11 + 10, 1].Style.GridLines.Bottom = fr;
                                }
                                else
                                {
                                    cRTable4.Cells[(n - 1) * 11 + 10, 1].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 2].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 3].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 4].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 5].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 6].Style.GridLines.Bottom = fr;
                                    cRTable4.Cells[(n - 1) * 11 + 10, 7].Style.GridLines.Bottom = fr;
                                }

                                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                                //cRArea.Children.Add(cRTable4);
                                cC1PrnDoc.Body.Children.Add(cRTable4);
                                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                            }
                        }
                    }

                    //振込先銀行がチェックされていた場合、出力
//                    if (Global.Prn_Frigin == 0)
                    if (Global.Prn_Frigin == 0 && Global.gcDataReader["SAIMU"].ToString() == "1")
                    {
                        int nCnt = Sel_Grp2_Info_Pre(Global.gcDataReader["TRCD"].ToString(), Global.gcDataReader["HJCD"].ToString());

                        if (nCnt > 0 && nLineCnt + 8 >= 62)     //←★処理中ページに１データも入らなければ改ページ処理するよう修正。
                        {
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();

                                cC1PrnDoc.Body.Children.Add(cRTable6);

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);

                                }
//-- <2016/03/14>
                                cRTable_H.Cells[2, 1].Text = sWork;

                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                            }
                        }
                        else
                        {
                            //改頁前の下線補完
                            RenderTable cRTable6 = new RenderTable();
                            cC1PrnDoc.Body.Children.Add(cRTable6);
                        }

                        //振込先銀行タブのデータを取得
                        if (nCnt > 0)
                        {

                            int iRoopCnt = nCnt;

                            RenderTable cRTable3 = new RenderTable();
                            cRTable3.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable3.Cols[0].Width = "20mm";
                            cRTable3.Cols[1].Width = "20mm";
                            cRTable3.Cols[2].Width = "20mm";
                            cRTable3.Cols[3].Width = "20mm";
                            cRTable3.Cols[4].Width = "20mm";
                            cRTable3.Cols[5].Width = "80mm";
                            cRTable3.Cols[1].CellStyle.Spacing.Left = "1mm";

                            cRTable3.Style.GridLines.All = fr;
                            cRTable3.Style.GridLines.Top = LineDef.Empty;
                            //振込先銀行タイトル
                            cRTable3.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable3.Cells[0, 0].Text = "振込先情報";

                            //振込先銀行タブのデータを取得
                            {
                                int n = 0;

                                int nSubDataIdx = 0;
                                while (nSubDataIdx < nCnt)
                                {

                                    n++;
                                    nSubDataIdx++;
                                    if (nLineCnt + 8 >= 62)
                                    {

                                        if (n > 1)
                                        {
                                            cRTable3.Cells[0, 0].SpanRows = 8 * (n - 1);
                                            cRTable3.Cells[(n - 1) * 8 -1, 1].Style.GridLines.Bottom = fr;
                                        }

                                        nLineCnt = 0;

                                        cC1PrnDoc.Body.Children.Add(cRTable3);

                                        RenderTable cRTableH = new RenderTable();
                                        cRTableH = (RenderTable)cRTableFuri.Clone();

                                        PrintDivisionResult eRet = PrintDivisionTrans();
                                        switch (eRet)
                                        {
                                            case PrintDivisionResult.NoDivision:            //
                                            case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                            case PrintDivisionResult.Unreached:             //
                                            case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                                break;
                                            case PrintDivisionResult.Preview:
                                            case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                            case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                            case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                                return null;
                                        }
                                        if (cC1PrnDoc.Body.Children.Count > 0)
                                        {
                                            cRTableH.BreakBefore = BreakEnum.Page;
                                        }
                                        cC1PrnDoc.Body.Children.Add(cRTableH);

                                        nLineCnt = 3;

                                        cRTable3 = new RenderTable();

                                        cRTable3.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                        cRTable3.Cols[0].Width = "20mm";
                                        cRTable3.Cols[1].Width = "20mm";
                                        cRTable3.Cols[2].Width = "20mm";
                                        cRTable3.Cols[3].Width = "20mm";
                                        cRTable3.Cols[4].Width = "20mm";
                                        cRTable3.Cols[5].Width = "80mm";
                                        cRTable3.Cols[1].CellStyle.Spacing.Left = "1mm";

                                        cRTable3.Style.GridLines.All = fr;
                                        cRTable3.Style.GridLines.Top = LineDef.Empty;
                                        //振込先銀行タイトル
                                        cRTable3.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                                        cRTable3.Cells[0, 0].Text = "振込先情報";

                                        n = 1;
                                    }

                                    for (int ii = (n - 1) * 8; ii < (n - 1) * 8 + 8; ii++)
                                    {
                                        cRTable3.Rows[ii].Height = "4mm";
                                    }

                                    //振込先銀行1行目
                                    cRTable3.Cells[(n - 1) * 8, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8, 1].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8, 1].Text = "ID:" + Global.drFRIGIN[nSubDataIdx - 1]["GIN_ID"].ToString().PadLeft(2, '0') + "                "
                                                                        + (Global.drFRIGIN[nSubDataIdx - 1]["FDEF"].ToString() == "1" ? "初期値" : "").PadRight(7, '　')
                                                                        + (Global.drFRIGIN[nSubDataIdx - 1]["DDEF"].ToString() == "1" ? "でんさい代表口座" : "");
                                    //振込先銀行2行目
                                    cRTable3.Cells[(n - 1) * 8 + 1, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 1, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 1, 1].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 1, 1].Text = "　[銀行]　　　　"
                                                                            + Global.drFRIGIN[nSubDataIdx - 1]["BANK_CD"].ToString().PadLeft(4) + " "
                                                                            + Global.drFRIGIN[nSubDataIdx - 1]["BKNAM"].ToString().PadRight(10, '　') + "　　　　　"
                                                                            + "[支店]　　"
                                                                            + Global.drFRIGIN[nSubDataIdx - 1]["SITEN_ID"].ToString().PadLeft(3) + " "
                                                                            + Global.drFRIGIN[nSubDataIdx - 1]["BRNAM"].ToString().PadRight(10, '　');

                                    //振込先銀行3行目
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 2].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 2].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 3].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 3].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 4].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 4].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 5].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 2, 5].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].Text = "　[預金種別]";
                                    if (Global.drFRIGIN[nSubDataIdx - 1]["YOKIN_TYP"].ToString() == "1")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　　　 普通";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["YOKIN_TYP"].ToString() == "2")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　　　 当座";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["YOKIN_TYP"].ToString() == "4")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　　　 貯蓄";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["YOKIN_TYP"].ToString() == "9")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　　　 他　";
                                    }
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　　　　　　　　　　　　[口座番号]";
                                    cRTable3.Cells[(n - 1) * 8 + 2, 1].Text += "　　" + Global.drFRIGIN[nSubDataIdx - 1]["KOUZA"].ToString();
                                    //振込先銀行4行目
                                    cRTable3.Cells[(n - 1) * 8 + 3, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 3, 1].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 2].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 2].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 3].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 3].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 4].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 4].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 5].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 3, 5].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 3, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 3, 1].Text = "　[口座名義人名称] 　" + Global.drFRIGIN[nSubDataIdx - 1]["MEIGI"].ToString().PadRight(30, '　');
                                    //振込先銀行5行目
                                    cRTable3.Cells[(n - 1) * 8 + 4, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 4, 1].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 2].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 2].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 3].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 3].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 4].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 4].Style.GridLines.Bottom = ld;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 5].Style.GridLines.Left = LineDef.Empty;
                                    //cRTable3.Cells[(n - 1) * 8 + 4, 5].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 4, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 4, 1].Text = "　[口座名義人カナ] 　" + Global.drFRIGIN[nSubDataIdx - 1]["MEIGIK"].ToString().PadRight(30, '　');

                                    //振込先銀行6行目
                                    cRTable3.Cells[(n - 1) * 8 + 5, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 5, 1].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 5, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 5, 1].Text = "　[手数料ID]　　 " + Global.drFRIGIN[nSubDataIdx - 1]["FTESUID"].ToString().PadLeft(2, ' ') + "　"
                                                                            + mcBsLogic.StringCut(Global.drFRIGIN[nSubDataIdx - 1]["TESUNAM"].ToString().PadRight(30, ' '), 30)
                                                                            + "[手数料負担]";
                                    if (Global.drFRIGIN[nSubDataIdx - 1]["TESUU"].ToString() == "1")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 5, 1].Text += "　自社負担";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["TESUU"].ToString() == "2")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 5, 1].Text += "　客先負担";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["TESUU"].ToString() == "3")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 5, 1].Text += "　折半";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["TESUU"].ToString() == "4")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 5, 1].Text += "　　　　　";
                                    }

                                    //振込先銀行7行目
                                    cRTable3.Cells[(n - 1) * 8 + 6, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 6, 1].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 6, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 6, 1].Text += "　[送金区分]";
                                    if (Global.drFRIGIN[nSubDataIdx - 1]["SOUKIN"].ToString() == "7")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 6, 1].Text += "　　　　 電信";
                                    }
                                    else if (Global.drFRIGIN[nSubDataIdx - 1]["SOUKIN"].ToString() == "8")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 6, 1].Text += "　　　　 文書";
                                    }
//-- <2016/03/10 0円なら金額印字しない>
//                                        cRTable3.Cells[(n - 1) * 8 + 6, 1].Text += "　　　　　　　　　　　　　[負担限度額]　" + Convert.ToDecimal(Global.drFRIGIN[nSubDataIdx - 1]["GENDO"].ToString()).ToString("#,#0").PadLeft(15, ' ') + "円";
                                    cRTable3.Cells[(n - 1) * 8 + 6, 1].Text += "　　　　　　　　　　　　　[負担限度額]　" + Convert.ToDecimal(Global.drFRIGIN[nSubDataIdx - 1]["GENDO"].ToString()).ToString("#,##").PadLeft(15, ' ') + "円";
//-- <2016/03/10>
                                    //振込先銀行8行目
                                    cRTable3.Cells[(n - 1) * 8 + 7, 1].Style.GridLines.Left = fr;
                                    cRTable3.Cells[(n - 1) * 8 + 7, 1].Style.GridLines.Bottom = ld;
                                    cRTable3.Cells[(n - 1) * 8 + 7, 1].SpanCols = 5;
                                    cRTable3.Cells[(n - 1) * 8 + 7, 1].Text += "　電子債権　　　　　 " + (Global.drFRIGIN[nSubDataIdx - 1]["DTESUSW"].ToString() == "1" ? "手数料設定を使用する　" : "手数料設定を使用しない");
                                    if (Global.drFRIGIN[nSubDataIdx - 1]["DTESUSW"].ToString() == "1")
                                    {
                                        cRTable3.Cells[(n - 1) * 8 + 7, 1].Text += "　　　　[手数料負担]";
                                        if (Global.drFRIGIN[nSubDataIdx - 1]["DTESU"].ToString() == "1")
                                        {
                                            cRTable3.Cells[(n - 1) * 8 + 7, 1].Text += "　自社負担";
                                        }
                                        else if (Global.drFRIGIN[nSubDataIdx - 1]["DTESU"].ToString() == "2")
                                        {
                                            cRTable3.Cells[(n - 1) * 8 + 7, 1].Text += "　客先負担";
                                        }
                                        else if (Global.drFRIGIN[nSubDataIdx - 1]["DTESU"].ToString() == "3")
                                        {
                                            cRTable3.Cells[(n - 1) * 8 + 7, 1].Text += "　折半";
                                        }
                                    }

                                    nLineCnt = nLineCnt + 8;
                                }

                                cRTable3.Cells[0, 0].SpanRows = 8 * n;
                                cRTable3.Cells[(n - 1) * 8 + 7, 1].Style.GridLines.Bottom = fr;

                                cC1PrnDoc.Body.Children.Add(cRTable3);
                            }
                        }
                    }

                    //その他がチェックされていた場合、出力
//                    if (Global.Prn_Others == 0)
                    // ▼#111516　竹内　2022/02/18
                    //if (Global.Prn_Others == 0 && Global.gcDataReader["SAIMU"].ToString() == "1")
                    if (Global.Prn_Others == 0)
                    // ▲#111516　竹内　2022/02/18
                    {
                        if (nLineCnt + 10 >= 62)
                        {
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();

                                cC1PrnDoc.Body.Children.Add(cRTable6);

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);

                                }
//-- <2016/03/14>

                                cRTable_H.Cells[2, 1].Text = sWork;

                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                            }
                        }

                        //その他タブのデータを取得
                        {
                            RenderTable cRTable7 = new RenderTable();

                            for (int j = 0; j < 10; j++)
                            {
                                cRTable7.Rows[j].Height = "4mm";
                                cRTable7.Rows[j].Style.TextAlignVert = AlignVertEnum.Center;
                            }
                            cRTable7.Cols[0].Width = "20mm";
                            cRTable7.Cols[1].Width = "160mm";
                            cRTable7.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTable7.Style.GridLines.All = fr;
                            cRTable7.Style.GridLines.Top = LineDef.Empty;
                            cRTable7.Cells[0, 0].SpanRows = 10;
                            cRTable7.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable7.Cells[0, 0].Text = "その他設定";

                            //その他1行目
                            string sNayose = "";
                            string sSetsuin = "";
                            string sStan = "";
                            sNayose = (Global.gcDataReader["NAYOSE"].ToString() == "1" ? "名寄せする　" : "名寄せしない");
                            sSetsuin = (Global.gcDataReader["F_SETUIN"].ToString() == "1" ? "節印実行する　" : "節印実行しない");
                            
                            //sStan = " [主担当者]　" + Global.gcDataReader["STAN"].ToString() + " " + Global.gcDataReader["STAN_NM"].ToString();
                            sStan = " [主担当者]　" + Global.gcDataReader["STAN"].ToString() + " " + mcBsLogic.Get_TNAM(Global.gcDataReader["STAN"].ToString());

                            cRTable7.Cells[0, 1].Text = sNayose + "　".PadRight(13, ' ') + sSetsuin + "　".PadRight(2, '　') + sStan;
                            cRTable7.Cells[0, 1].Style.GridLines.Bottom = ld;

                            //その他2行目
                            string sKamoku = "";
                            string sBumon = "";
                            sKamoku = "　[科目]　　　" + Global.gcDataReader["KCOD"].ToString() + "　　　" + Global.gcDataReader["O_KNAM"].ToString();
                            sBumon = " [部門]　     " + Global.gcDataReader["SBCOD"].ToString() + "　　 " + Global.gcDataReader["O_BNAM"].ToString();
                            cRTable7.Cells[1, 1].Text = sKamoku + "　".PadRight(2, '　') + sBumon + "　".PadRight(3, '　');
                            cRTable7.Cells[1, 1].Style.GridLines.Bottom = ld;

                            //その他3行目
                            string sF_SOUFU = "";
                            string sANNAI = "";
                            string sTSOKBN = "";
                            //string sSZEI = "";
                            string sTEGVAL = "";
                            if (Global.gcDataReader["F_SOUFU"].ToString() == "0")
                            {
                                sF_SOUFU = "送付しない";
                            }
                            else if (Global.gcDataReader["F_SOUFU"].ToString() == "1")
                            {
                                sF_SOUFU = "書留";
                            }
                            else if (Global.gcDataReader["F_SOUFU"].ToString() == "2")
                            {
                                sF_SOUFU = "簡易書留";
                            }
                            else if (Global.gcDataReader["F_SOUFU"].ToString() == "3")
                            {
                                sF_SOUFU = "";
                            }
                            if (Global.gcDataReader["ANNAI"].ToString() == "1")
                            {
                                sANNAI = "パターン１";
                            }
                            else if (Global.gcDataReader["ANNAI"].ToString() == "2")
                            {
                                sANNAI = "パターン２";
                            }
                            if (Global.gcDataReader["TSOKBN"].ToString() == "0")
                            {
                                sTSOKBN = "来社";
                            }
                            else if (Global.gcDataReader["TSOKBN"].ToString() == "1")
                            {
                                sTSOKBN = "自社負担";
                            }
                            else if (Global.gcDataReader["TSOKBN"].ToString() == "2")
                            {
                                sTSOKBN = "客先負担";
                            }
                            //if (Global.gcDataReader["SZEI"].ToString() == "0")
                            //{
                            //    sSZEI = "切り捨て";
                            //}
                            //else if (Global.gcDataReader["SZEI"].ToString() == "1")
                            //{
                            //    sSZEI = "四捨五入";
                            //}
                            //else if (Global.gcDataReader["SZEI"].ToString() == "2")
                            //{
                            //    sSZEI = "切り上げ";
                            //}
                            if (Global.gcDataReader["TEGVAL"].ToString() != "")
                            {
//-- <2016/03/10 フォーマットを変更>
//                                sTEGVAL = Convert.ToDecimal(Global.gcDataReader["TEGVAL"].ToString()).ToString("#,##0").PadLeft(5, ' ') + "円";
                                sTEGVAL = Convert.ToDecimal(Global.gcDataReader["TEGVAL"].ToString()).ToString("#,###").PadLeft(5, ' ') + "円";
//-- <2016/03/10>
                            }
                            cRTable7.Cells[2, 1].Text = "　[送付案内]　" + sF_SOUFU.PadRight(5, '　')
                                                      + " [案内文] " + sANNAI.PadRight(5, '　')
                                                      + " [負担区分]   " + sTSOKBN.PadRight(5, '　')
                                                      + "  [送料]  " + "　　" + sTEGVAL;
                            cRTable7.Cells[2, 1].Style.GridLines.Bottom = ld;

                            //その他4行目
//-- <2016/03/10 0印字しない対応>
                            string sHojo1 = "";
                            string sHojo2 = "";
                            string sHojo3 = "";

                            if (Global.gcDataReader["DM1"].ToString() == "0")
                            {
                                sHojo1 = "";
                            }
                            else { sHojo1 = Global.gcDataReader["DM1"].ToString(); }

                            if (Global.gcDataReader["DM2"].ToString() == "0")
                            {
                                sHojo2 = "";
                            }
                            else { sHojo2 = Global.gcDataReader["DM2"].ToString(); }

                            if (Global.gcDataReader["DM3"].ToString() == "0")
                            {
                                sHojo3 = "";
                            }
                            else { sHojo3 = Global.gcDataReader["DM3"].ToString(); }

//                            cRTable7.Cells[3, 1].Text = "　[補助ｺｰﾄﾞ1] " + Global.gcDataReader["DM1"].ToString().PadRight(20) + " 　　　　　[補助ｺｰﾄﾞ2]  " + Global.gcDataReader["DM2"].ToString();
                            cRTable7.Cells[3, 1].Text = "　[補助ｺｰﾄﾞ1] " + sHojo1.PadRight(20) + " 　　　　　[補助ｺｰﾄﾞ2]  " + sHojo2;

//-- <2016/03/10>
                            cRTable7.Cells[3, 1].Style.GridLines.Bottom = ld;

                            //その他5行目
//-- <2106/03/10 0印字しない対応>
//                            cRTable7.Cells[4, 1].Text = "  [補助ｺｰﾄﾞ3] " + Global.gcDataReader["DM3"].ToString().PadRight(20) + "　　　　　"
                            cRTable7.Cells[4, 1].Text = "  [補助ｺｰﾄﾞ3] " + sHojo3.PadRight(20) + "　　　　　"
//-- <2016/03/10>
                                                      + " [仕入先番号] " + Global.gcDataReader["CDM2"].ToString().PadRight(10)
                                                      + "　[支払通知] " + (Global.gcDataReader["F_SHITU"].ToString() == "0" ? "印刷しない" : "印刷する");
                            cRTable7.Cells[4, 1].Style.GridLines.Bottom = ld;

                            if (Global.gcDataReader["GENSEN"].ToString() == "0")
                            {
                                //その他6行目
                                cRTable7.Cells[5, 1].Text = "源泉税計算しない";
                                cRTable7.Cells[6, 1].Text = "";
                                cRTable7.Cells[7, 1].Text = "";
                            }
                            else if (Global.gcDataReader["GENSEN"].ToString() == "3")
                            {
                                //その他6行目
                                cRTable7.Cells[5, 1].Text = "計算なしで支払調書を出力する";
                                //その他7行目
                                cRTable7.Cells[6, 1].Text = "";
                                //その他8行目
                                if (Global.gcDataReader["GOU"].ToString() == "1")
                                {
                                    cRTable7.Cells[7, 1].Text = "　[号(第204条第一項)]　原稿料・作曲料等";
                                }
                                else if (Global.gcDataReader["GOU"].ToString() == "2")
                                {
                                    cRTable7.Cells[7, 1].Text = "　[号(第204条第一項)]　弁護士・税理士等";
                                }
                                else
                                {
                                    cRTable7.Cells[7, 1].Text = "　　　　　　　　　　　　　　　　　　　　 ";
                                }
                                cRTable7.Cells[7, 1].Text += "　" + "[源泉区分]　" + Global.gcDataReader["GGKBNM"].ToString().PadRight(8, '　') + " [支払区分]　" + Global.gcDataReader["O_SKBNM"].ToString();
                            }
                            else 
                            {
                                //その他6行目
                                cRTable7.Cells[5, 1].Text = "源泉税計算する  ";
                                cRTable7.Cells[5, 1].Text += "　".PadRight(14, '　') +" [計算基準]";
                                if (Global.gcDataReader["GSSKBN"].ToString() == "1")
                                {
                                    cRTable7.Cells[5, 1].Text += "　 支払金額";
                                }
                                else
                                {
                                    cRTable7.Cells[5, 1].Text += "　 税抜金額";
                                }

                                //その他7行目
                                if (Global.gcDataReader["GENSEN"].ToString() == "1")
                                {
                                    cRTable7.Cells[6, 1].Text = "　[計算式]　　支払金額×10.21%(但し、100万超の部分20.42%)";
                                }
                                else if (Global.gcDataReader["GENSEN"].ToString() == "2")
                                {
                                    cRTable7.Cells[6, 1].Text = "　[計算式]　　(支払金額－１万)×10.21%";
                                }

                                //その他8行目
                                if (Global.gcDataReader["GOU"].ToString() == "1")
                                {
                                    cRTable7.Cells[7, 1].Text = "　[号(第204条第一項)]　原稿料・作曲料等";
                                }
                                else if (Global.gcDataReader["GOU"].ToString() == "2")
                                {
                                    cRTable7.Cells[7, 1].Text = "　[号(第204条第一項)]　弁護士・税理士等";
                                }
                                else
                                {
                                    cRTable7.Cells[7, 1].Text = "　　　　　　　　　　　　　　　　　　　　 ";
                                }
                                cRTable7.Cells[7, 1].Text += "　" + "[源泉区分]　" + Global.gcDataReader["GGKBNM"].ToString().PadRight(8, '　') + " [支払区分]　" + Global.gcDataReader["O_SKBNM"].ToString();

                            }

                            cRTable7.Cells[5, 1].Style.GridLines.Bottom = ld;
                            cRTable7.Cells[6, 1].Style.GridLines.Bottom = ld;
                            cRTable7.Cells[7, 1].Style.GridLines.Bottom = ld;

                            if (Global.gcDataReader["HORYU"].ToString() == "0")
                            {
                                //その他9行目
                                cRTable7.Cells[8, 1].Text = "控除関連　支払保留を使用しない";
                                cRTable7.Cells[9, 1].Text = "";
                            }
                           else
                            {
                                //その他9行目
                                if (Global.gcDataReader["HORYU"].ToString() == "1")
                                {
                                    cRTable7.Cells[8, 1].Text = "控除関連　支払保留を使用する　　[適用基準額]";
                                }
                                else if (Global.gcDataReader["HORYU"].ToString() == "2")
                                {
                                    cRTable7.Cells[8, 1].Text = "控除関連　自動控除を使用する　　[適用基準額]";
                                }
                                cRTable7.Cells[8, 1].Text += Convert.ToDecimal(Global.gcDataReader["HR_KIJYUN"].ToString()).ToString("#,##0").PadLeft(15, ' ') + "円以上";
//-- <2016/04/02>
//                                cRTable7.Cells[8, 1].Text += "　　 [計算区分]　" + (Global.gcDataReader["HORYU_F"].ToString() == "0" ? "0:比率" : "1:定額");
                                cRTable7.Cells[8, 1].Text += "　　 [計算区分]　" + (Global.gcDataReader["HORYU_F"].ToString() == "1" ? "1:比率" : "2:定額");
//-- <2016/04/02>

                                //その他10行目
                                cRTable7.Cells[9, 1].Text = "　[比率]　" + Convert.ToDecimal(Global.gcDataReader["HOVAL"].ToString()).ToString("##0.000").PadLeft(7, ' ') + "%";
                                cRTable7.Cells[9, 1].Text += "　　　　　　　" + "[定額]　　　" + Convert.ToDecimal(Global.gcDataReader["HRORYUGAKU"].ToString()).ToString("#,##0").PadLeft(15, ' ') + "円";
                                cRTable7.Cells[9, 1].Text += "　　　　 [作成区分]　" + Global.gcDataReader["O_SKBNM2"].ToString();
                            }
                            cRTable7.Cells[8, 1].Style.GridLines.Bottom = ld;
                            cRTable7.Cells[9, 1].Style.GridLines.Bottom = fr;

                            nLineCnt = nLineCnt + 10;

                            cC1PrnDoc.Body.Children.Add(cRTable7);
                        }
                    }

                    //外貨がチェックされていた場合、出力
//                    if (Global.Prn_Gaika == 0)
                    if (Global.Prn_Gaika == 0 && Global.gcDataReader["SAIMU"].ToString() == "1" && Global.gcDataReader["GAI_F"].ToString() == "1")
                    {
                        if (nLineCnt + 11 >= 62)
                        {
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();

                                cC1PrnDoc.Body.Children.Add(cRTable6);

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);

                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[2, 1].Text = sWork;

                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                            }
                        }

                        //外貨のデータを取得
                        {
                            RenderTable cRTableG = new RenderTable();

                            for (int j = 0; j < 11; j++)
                            {
                                cRTableG.Rows[j].Height = "4mm";
                                cRTableG.Rows[j].Style.TextAlignVert = AlignVertEnum.Center;
                            }
                            cRTableG.Cols[0].Width = "20mm";
                            cRTableG.Cols[1].Width = "160mm";
                            cRTableG.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTableG.Style.GridLines.All = fr;
                            cRTableG.Style.GridLines.Top = LineDef.Empty;
                            cRTableG.Cells[0, 0].SpanRows = 11;
                            cRTableG.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTableG.Cells[0, 0].Text = "外貨設定";

                            //外貨設定1行目
                            cRTableG.Cells[0, 1].Text = "　[締日]　　　都度　　　　　　　   [支払区分]　　 外貨送金　　　　　　[取引通貨]  " + Global.gcDataReader["HEI_CD"].ToString();
                            cRTableG.Cells[0, 1].Style.GridLines.Bottom = ld;

                            //外貨設定2行目
                            cRTableG.Cells[1, 1].Text = "　[送金種類]　" + (Global.gcDataReader["GAI_SF"].ToString() == "0" ? "電信送金　" : "送金小切手") + "　　　　   [送金支払方法] " + (Global.gcDataReader["GAI_SH"].ToString() == "0" ? "通知払" : "請求払");
                            cRTableG.Cells[1, 1].Style.GridLines.Bottom = ld;

                            //外貨設定3行目
                            cRTableG.Cells[2, 1].Text = "　[出金口座]　" + Global.gcDataReader["G_OWNBKCOD"].ToString() + "　" + Global.gcDataReader["G_BKNAM"].ToString().PadRight(10, '　')
                                                      + "　　 " + Global.gcDataReader["G_OWNBRCOD"].ToString() + "　" + Global.gcDataReader["G_BRNAM"].ToString().PadRight(10, '　')
                                                      + " " + Get_YokinType_NM(Global.gcDataReader["G_YOKINKIND"].ToString()) + "　 " + Global.gcDataReader["G_KOZANO"].ToString()
                                                      + "   " + Global.gcDataReader["G_HEI_CD"].ToString();
                            cRTableG.Cells[2, 1].Style.GridLines.Bottom = ld;

                            //外貨設定4行目
//-- < "">
//                            cRTableG.Cells[3, 1].Text = "　[手数料負担]　　　" + Get_Tesuu_NM(Convert.ToInt32(Global.gcDataReader["GAI_TF"].ToString()));
                            cRTableG.Cells[3, 1].Text = "　[手数料負担]　　　" + Get_Tesuu_NM(Convert.ToInt32(Global.gcDataReader["GAI_TF"].ToString() == "" ? "0" : Global.gcDataReader["GAI_TF"].ToString()));
//-- 
                            cRTableG.Cells[3, 1].Style.GridLines.Bottom = ld;

                            //外貨設定5行目
                            cRTableG.Cells[4, 1].Text = "　[PAYEE NAME]　　　" +Global.gcDataReader["ENG_NAME"].ToString();
                            cRTableG.Cells[4, 1].Style.GridLines.Bottom = ld;

                            //外貨設定6行目
                            cRTableG.Cells[5, 1].Text = "　[ADDRESS]　   　　" + Global.gcDataReader["ENG_ADDR"].ToString();
                            cRTableG.Cells[5, 1].Style.GridLines.Bottom = ld;

                            //外貨設定7行目
                            cRTableG.Cells[6, 1].Text = "　[口座番号/IBANｺｰﾄﾞ]　" + Global.gcDataReader["ENG_KZNO"].ToString();
                            cRTableG.Cells[6, 1].Style.GridLines.Bottom = ld;

                            //外貨設定8行目
                            cRTableG.Cells[7, 1].Text = "　[SWIFT(BIC)ｺｰﾄﾞ]　" + Global.gcDataReader["ENG_SWIF"].ToString();
                            cRTableG.Cells[7, 1].Style.GridLines.Bottom = ld;

                            //外貨設定9行目
                            cRTableG.Cells[8, 1].Text = "　[被仕向銀行名]  　" + Global.gcDataReader["ENG_BNKNAM"].ToString();
                            cRTableG.Cells[8, 1].Style.GridLines.Bottom = ld;

                            //外貨設定10行目
                            cRTableG.Cells[9, 1].Text = "　[被仕向支店名]　  " + Global.gcDataReader["ENG_BRNNAM"].ToString();
                            cRTableG.Cells[9, 1].Style.GridLines.Bottom = ld;

                            //外貨設定11行目
                            cRTableG.Cells[10, 1].Text = "　[被仕向銀行住所]　" + Global.gcDataReader["ENG_BNKADDR"].ToString();
                            //cRTableG.Cells[10, 1].Style.GridLines.Bottom = ld;

                            nLineCnt = nLineCnt + 11;

                            cC1PrnDoc.Body.Children.Add(cRTableG);
                        }
                    }
                    //ﾏｽﾀｰ情報がチェックされていた場合、出力
                    if (Global.Prn_Master == 0)
                    {
                        if (nLineCnt + 2 >= 62)
                        {
                            {
                                //改頁前の下線補完
                                RenderTable cRTable6 = new RenderTable();

                                cC1PrnDoc.Body.Children.Add(cRTable6);

                                RenderTable cRTable_H = new RenderTable();
                                //改頁を挿入

                                PrintDivisionResult eRet = PrintDivisionTrans();
                                switch (eRet)
                                {
                                    case PrintDivisionResult.NoDivision:            //
                                    case PrintDivisionResult.BeforeRange:           //ページ範囲外によりブレイク。
                                    case PrintDivisionResult.Unreached:             //
                                    case PrintDivisionResult.PreviewContinue:       //続行。次の印刷グループへ。
                                        break;
                                    case PrintDivisionResult.Preview:
                                    case PrintDivisionResult.PreviewRestart:        //先頭より再処理
                                    case PrintDivisionResult.PreviewStop:           //プレビューから終了
                                    case PrintDivisionResult.AfterRange:            //ページ範囲外によりブレイク。最終印刷グループの処理。
                                        return null;
                                }
                                if (cC1PrnDoc.Body.Children.Count > 0)
                                {
                                    cRTable_H.BreakBefore = BreakEnum.Page;
                                }

                                cRTable_H.Rows[0].Height = "6mm";
                                cRTable_H.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[1].Height = "4mm";
                                cRTable_H.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                                cRTable_H.Rows[2].Height = "4mm";
                                cRTable_H.Rows[2].Style.TextAlignVert = AlignVertEnum.Center;

                                cRTable_H.Cols[0].Width = "35mm";
                                cRTable_H.Cols[1].Width = "110mm";
                                cRTable_H.Cols[2].Width = "35mm";

                                cRTable_H.Cols[0].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Cols[1].CellStyle.Spacing.Left = "1mm";
                                cRTable_H.Style.GridLines.Top = fr;
                                cRTable_H.Style.GridLines.Left = fr;
                                cRTable_H.Style.GridLines.Right = fr;
                                cRTable_H.Style.GridLines.Bottom = fr;
                                string sWork;

                                //ヘッダー1行目
                                cRTable_H.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                string sTRCD = "";
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["TRCD"].ToString();
                                }
                                if (sTRCD.Length == 13)
                                {
                                    cRTable_H.Cells[0, 0].Text = "";
                                }
                                else
                                {
                                    cRTable_H.Cells[0, 0].Text = (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
                                cRTable_H.Cells[0, 1].Style.FontSize = 12;
                                cRTable_H.Cells[0, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                cRTable_H.Cells[0, 1].Text = Global.gcDataReader["TORI_NAM"].ToString().PadRight(22, '　');
                                cRTable_H.Cells[0, 2].Style.TextAlignHorz = AlignHorzEnum.Center;
                                cRTable_H.Cells[0, 2].Text = (Global.gcDataReader["STFLG"].ToString() == "1" ? "取引停止" : "        ");
                                //ヘッダー2行目
                                cRTable_H.Cells[1, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/10 文言修正>
//                                cRTable_H.Cells[1, 0].Text = (Global.gcDataReader["TGASW"].ToString() == "0" ? "                  " : "手形管理のみで使用");
                                cRTable_H.Cells[1, 0].Text = (Global.gcDataReader["TGASW"].ToString() == "0" ? "                  " : "期日管理のみで使用");
//-- <2016/03/10>
                                sWork = "得意先:";
                                if (Global.gcDataReader["SAIKEN"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "2")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[1, 0].Text = sWork;

                                cRTable_H.Cells[1, 1].SpanCols = 2;
                                cRTable_H.Cells[1, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
//-- <2016/03/14>
//                                string sWork = Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' ') + " "
//                                                + Global.gcDataReader["KNLD"].ToString().PadRight(4) + "  "
//                                                + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "入金代表者：" : "入金代表者");
                                sWork = StringCut(Global.gcDataReader["TRFURI"].ToString().PadRight(44, ' '), 44) + " "
                                      + StringCut(Global.gcDataReader["KNLD"].ToString().PadRight(4), 4) + "  "
                                      + (Global.gcDataReader["SAIKEN_FLG"].ToString() == "0" ? "" : "入金代表者");
//-- <2016/03/14>
                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["NYDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "入金代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["NYDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>
                                cRTable_H.Cells[1, 1].Text = sWork;

                                cRTable_H.Cells[2, 0].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = "仕入先:";
                                if (Global.gcDataReader["SAIMU"].ToString() == "1")
                                {
                                    sWork += "○";
                                }
                                else if (Global.gcDataReader["TGASW"].ToString() == "1" || Global.gcDataReader["TGASW"].ToString() == "3")
                                {
                                    sWork += "期日管理のみ";
                                }
                                else
                                {
                                    sWork += "－";
                                }
                                cRTable_H.Cells[2, 0].Text = sWork;

                                cRTable_H.Cells[2, 1].SpanCols = 2;
                                cRTable_H.Cells[2, 1].Style.TextAlignHorz = AlignHorzEnum.Left;
                                sWork = StringCut(Global.gcDataReader["RYAKU"].ToString().PadRight(20, ' '), 20) + "    ";
                                if (Global.gcDataReader["GRPID"].ToString() != "0")
                                {
                                    sWork += Global.gcDataReader["GRPID"].ToString().PadLeft(2, ' ') + ":" + StringCut(Global.gcDataReader["GRPNM"].ToString().PadRight(20, ' '), 20)
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            +(Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }
                                else
                                {
                                    sWork += " ".PadRight(23, ' ')
                                            + "    "
//-- <2016/03/14>
//                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "支払代表者：" : "支払代表者");
                                            + (Global.gcDataReader["SAIMU_FLG"].ToString() == "0" ? "" : "支払代表者");
//-- <2016/03/14>
                                }

                                if ((Global.nTRCD_Type == 0) &&
                                    (Global.nTRCD_ZE == 1))
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimStart('0');
                                }
                                else if (Global.nTRCD_Type == 1)
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString().TrimEnd(' ');
                                }
                                else
                                {
                                    sTRCD = Global.gcDataReader["SIDAICD"].ToString();
                                }
//-- <2016/03/14>
//                                sWork += (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                if (sTRCD != "")
                                {
                                    sWork += "支払代表者：" + (Global.nTRCD_HJ == 1 ? sTRCD + "-" + Global.gcDataReader["SIDAIHJCD"].ToString().PadLeft(6, '0') : sTRCD);
                                }
//-- <2016/03/14>

                                cRTable_H.Cells[2, 1].Text = sWork;

                                nLineCnt = 3;
                                cRTableFuri = (RenderTable)cRTable_H.Clone();

                                cC1PrnDoc.Body.Children.Add(cRTable_H);
                            }
                        }

                        {
                            RenderTable cRTable8 = new RenderTable();

                            cRTable8.Rows[0].Height = "4mm";
                            cRTable8.Rows[0].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable8.Rows[1].Height = "4mm";
                            cRTable8.Rows[1].Style.TextAlignVert = AlignVertEnum.Center;
                            cRTable8.Cols[0].Width = "20mm";
                            cRTable8.Cols[1].Width = "160mm";
                            cRTable8.Cols[1].CellStyle.Spacing.Left = "1mm";
                            cRTable8.Style.GridLines.All = fr;
                            cRTable8.Style.GridLines.Top = LineDef.Empty;
                            //ﾏｽﾀｰ情報タイトル
                            cRTable8.Cells[0, 0].SpanRows = 2;
                            cRTable8.Cells[0, 0].Style.TextAlignHorz = AlignHorzEnum.Center;
                            cRTable8.Cells[0, 0].Text = "ﾏｽﾀｰ情報";
                            //ﾏｽﾀｰ情報1行目
                            string sZSTYMD = "";
                            string sZEDYMD = "";
                            string sSTYMD = "";
                            string sEDYMD = "";
                            if (Global.gcDataReader["ISTAYMD"].ToString().Length == 8)
                            {
                                sZSTYMD = Global.gcDataReader["ISTAYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }
                            if (Global.gcDataReader["IENDYMD"].ToString().Length == 8)
                            {
                                sZEDYMD = Global.gcDataReader["IENDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }
                            if (Global.gcDataReader["STYMD"].ToString().Length == 8)
                            {
                                sSTYMD = Global.gcDataReader["STYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }
                            if (Global.gcDataReader["EDYMD"].ToString().Length == 8)
                            {
                                sEDYMD = Global.gcDataReader["EDYMD"].ToString().Insert(6, "/").Insert(4, "/");
                            }

                            cRTable8.Cells[0, 1].Text = "(債権債務) "
                                                      + "[使用開始日] " + sSTYMD.PadRight(15)
                                                      + "[使用終了日] " + sEDYMD.PadRight(10);
                            cRTable8.Cells[0, 1].Style.GridLines.Bottom = ld;
                            cRTable8.Cells[1, 1].Text = "(財務管理) "
                                                      + "[入力開始日] " + sZSTYMD.PadRight(15)
                                                      + "[入力終了日] " + sZEDYMD.PadRight(10);
                            nLineCnt = nLineCnt + 2;

                            cC1PrnDoc.Body.Children.Add(cRTable8);
                        }
                    }

                    //空行出力用
                    RenderTable cRTable9 = new RenderTable();

                    //**
                    //**cRTable9.Width = "181.5mm";
                    //cRTable9.Width = "180mm";
                    //**
                    cRTable9.Rows[0].Height = "4mm";
                    cRTable9.Style.GridLines.All = LineDef.Empty;
                    cRTable9.Cells[0, 0].Text = "　";
                    nLineCnt = nLineCnt+ 1;

                    //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                    //cRArea.Children.Add(cRTable9);
                    cC1PrnDoc.Body.Children.Add(cRTable9);
                    //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

                }

                //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                if (Global.dtTORI != null)
                {
                    Global.dtTORI.Clear();
                    Global.dtTORI.Dispose();
                    Global.dtTORI = null;
                }
                if (Global.Prn_Frigin == 0)
                {
                    if (Global.dtFRIGIN != null)
                    {
                        Global.dtFRIGIN.Clear();
                        Global.dtFRIGIN.Dispose();
                        Global.dtFRIGIN = null;
                    }
                    Global.drFRIGIN = null;
                }
                if (Global.Prn_Shiharai == 0)
                {
                    if (Global.dtTSHOH_SJ != null)
                    {
                        Global.dtTSHOH_SJ.Clear();
                        Global.dtTSHOH_SJ.Dispose();
                        Global.dtTSHOH_SJ = null;
                    }
                    Global.drTSHOH_SJ = null;
                }
                GC.Collect();
                //2013/07/16 ICS.居軒 ▲パフォーマンス改善
                #endregion
            }
            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
            //return cRArea;
            return null;
            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

        }

        /// <summary>
        /// 取引先リストに出力するデータを取得
        /// </summary>
        private void Sel_TRCD_List()
        {
            #region 取引先一覧表のデータ取得SQL作成
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }

                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //string sFuncLength = DbCls.DbType == DbCls.eDbType.Oracle ? "LENGTH" : "LEN";
                string sFuncLength = IcsComUtil.ComUtil.IsPostgreSQL() ? "LENGTH" : "LEN";
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

                //if (Global.nTRCD_HJ == 0)
                //{
                //    //where句の生成
                //    string sSqlWhere = "";
                //    if ((Global.Prn_TRCD_Sta != "") &&
                //        (Global.Prn_TRCD_End != ""))
                //    {
                //        sSqlWhere = " WHERE TRCD BETWEEN '" + Global.Prn_TRCD_Sta + "' AND '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(TRCD) < 13 ";
                //    }
                //    else if ((Global.Prn_TRCD_Sta != "") &&
                //             (Global.Prn_TRCD_End == ""))
                //    {
                //        sSqlWhere = " WHERE TRCD >= '" + Global.Prn_TRCD_Sta + "' AND " + sFuncLength + "(TRCD) < 13 ";
                //    }
                //    else if ((Global.Prn_TRCD_Sta == "") &&
                //             (Global.Prn_TRCD_End != ""))
                //    {
                //        sSqlWhere = " WHERE TRCD <= '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(TRCD) < 13 ";
                //    }
                //    else
                //    {
                //        sSqlWhere = " WHERE " + sFuncLength + "(TRCD) < 13 ";

                //    }

                //    //取引先の検索SQL生成&実行
                //    Global.cCmdSel.CommandText = "SELECT * FROM SS_TORI " + sSqlWhere;
                //    if (Global.Prn_SortKEY == 0)
                //    {
                //        Global.cCmdSel.CommandText += "ORDER BY TRCD, HJCD ";
                //    }
                //    else
                //    {
                //        Global.cCmdSel.CommandText += "ORDER BY KNLD ";
                //    }
                //    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
                //}
                string sCmd1 = "";
                string sCmd2 = "";

                //where句の生成
                string sSqlWhere = "";
                string sWhere1 = "";
                string sWhere2 = "";

                sSqlWhere = " WHERE ";

                string sSubwhere = " AND T.TRCD = COALESCE(ST.TRCD, ' ') ";

                if (Global.Prn_PKind == 2 || Global.Prn_PKind == 0)
                {
                    if (Global.Prn_PType == 1 || Global.Prn_PType == 0)
                    {
                        if ((Global.Prn_TRCD_Sta != "") &&
                            (Global.Prn_HJCD_Sta != "") &&
                            (Global.Prn_TRCD_End != "") &&
                            (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta
                                                       + "')) AND (COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End + "' OR (COALESCE(ST.TRCD, ' ') = '"
                                                       + Global.Prn_TRCD_End + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "')) "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta != "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta
                                                       + "')) AND COALESCE(ST.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta != "") &&
                                 (Global.Prn_TRCD_End == "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND ("
                                                       + "COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_End
                                                       + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "')) AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') BETWEEN '" + Global.Prn_TRCD_Sta + "' AND '"
                                                       + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End == "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta == "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_End + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta == "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else
                        {
                            sWhere1 = " (" + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                    }
                    if (Global.Prn_PType == 2 || Global.Prn_PType == 0)
                    {
                        if (Global.Prn_TRCD_Once != "")
                        {
                            sWhere2 = string.Format("( ST.TORI_NAM like '%{0}%' AND ", Global.Prn_TRCD_Once);
                        }
                        else
                        {
                            sWhere2 = " ( ";
                        }
                        sWhere2 += " ( " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) = 13 ) ";

                    }
                    if (Global.Prn_PType == 1)
                    {
                        sSqlWhere += sWhere1;
                    }
                    else if (Global.Prn_PType == 2)
                    {
                        sSqlWhere += sWhere2;
                    }
                    else
                    {
                        sSqlWhere += " ( " + sWhere1 + " OR " + sWhere2 + " ) ";
                    }

                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + " )";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + " )";
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + " )";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                        else  if (Global.Prn_ZSTYMD_End != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + " )";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + " )";
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + " )";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                    }
                    else
                    {
//-- <2016/03/21>
//                        sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                        sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    if (!Global.Prn_ZEDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                        else if (Global.Prn_ZEDYMD_Sta != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + " )";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + " )";
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + " )";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                        else if (Global.Prn_ZEDYMD_End != 0)
                        {
//-- <2016/03/21>
//                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + " )";
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + " )";
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + " )";
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                        }
                    }
                    else
                    {
//-- <2016/03/21>
//                        sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE T.IENDYMD = 0 " + sSubwhere + ") ";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.IENDYMD = 0 " + sSubwhere + ") ";
                        sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD = 0 " + sSubwhere + ") ";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    if (!Global.Prn_STYMD_Null)
                    {
                        if (Global.Prn_STYMD_Sta != 0 && Global.Prn_STYMD_End != 0)
                        {
                            sSqlWhere += " AND ST.STYMD BETWEEN " + Global.Prn_STYMD_Sta + " AND " + Global.Prn_STYMD_End;
                        }
                        else if (Global.Prn_STYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ST.STYMD >= " + Global.Prn_STYMD_Sta + " AND ST.STYMD <> 0)";
                        }
                        else if (Global.Prn_STYMD_End != 0)
                        {
                            sSqlWhere += " AND (ST.STYMD <= " + Global.Prn_STYMD_End + " OR ST.STYMD = 0)";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ST.STYMD = 0 ";
                    }
                    if (!Global.Prn_EDYMD_Null)
                    {
                        if (Global.Prn_EDYMD_Sta != 0 && Global.Prn_EDYMD_End != 0)
                        {
                            sSqlWhere += " AND ST.EDYMD BETWEEN " + Global.Prn_EDYMD_Sta + " AND " + Global.Prn_EDYMD_End;
                        }
                        else if (Global.Prn_EDYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ST.EDYMD >= " + Global.Prn_EDYMD_Sta + " OR ST.EDYMD = 0)";
                        }
                        else if (Global.Prn_EDYMD_End != 0)
                        {
                            sSqlWhere += " AND (ST.EDYMD <= " + Global.Prn_EDYMD_End + " AND ST.EDYMD <> 0)";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ST.EDYMD = 0 ";
                    }

                    if (Global.PrnTarget == 1)
                    {
                        sSqlWhere += " AND ST.SAIKEN = 1 ";
                    }
                    else if (Global.PrnTarget == 2)
                    {
                        sSqlWhere += " AND ST.SAIMU = 1 ";
                    }
                    else if (Global.PrnTarget == 3)
                    {
                        sSqlWhere += " AND ST.TGASW >= 1 ";
                    }

                    //取引先の検索SQL生成&実行
                    sCmd1 = "SELECT ST.TRCD TRCD, ST.HJCD HJCD, ST.RYAKU RYAKU, ST.TORI_NAM TORI_NAM, ST.KNLD KNLD, ";
//-- <2016/03/21>
//                    sCmd1 += " ST.TGASW TGASW, CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z1500..TRNAM Z WHERE COALESCE(Z.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > '0' THEN '1' ELSE 0 END ZFLG, '1' SSFLG, ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sCmd1 += " ST.TGASW TGASW, CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z" + Global.sCcod + "..TRNAM Z WHERE COALESCE(Z.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > '0' THEN '1' ELSE 0 END ZFLG, '1' SSFLG, ";
                    sCmd1 += " ST.TGASW TGASW, CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRNAM Z WHERE COALESCE(Z.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > '0' THEN '1' ELSE 0 END ZFLG, '1' SSFLG, ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    sCmd1 += " ZT2.ISTAYMD ISTAYMD, ZT2.IENDYMD IENDYMD, ST.STYMD STYMD, ST.EDYMD EDYMD, ";
//-- <2016/03/21>
//                    sCmd1 += " CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z1500..TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'SS' TYP ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sCmd1 += " CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z" + Global.sCcod + "..TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'SS' TYP ";
                    sCmd1 += " CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'SS' TYP ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    //sCmd1 += " ,CASE WHEN " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) = 13 THEN '1' ELSE '0' END VFLG FROM SS_TORI ST ";
                    sCmd1 += ", ST.TRFURI, SAIKEN, SAIMU, SAIKEN_FLG, SAIMU_FLG, GRPID";
                    sCmd1 += ", DH.SIDAICD, DH.SIDAIHJCD, SI.NYDAICD, SI.NYDAIHJCD";
                    sCmd1 += " FROM SS_TORI ST ";
//-- <2016/03/21>
//                    sCmd1 += " LEFT JOIN ICSP_312Z1500..TRNAM ZT2 ON COALESCE(ST.TRCD, ' ') = COALESCE(ZT2.TRCD, ' ') ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sCmd1 += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..TRNAM ZT2 ON COALESCE(ST.TRCD, ' ') = COALESCE(ZT2.TRCD, ' ') ";
                    sCmd1 += " LEFT JOIN " + Global.sZJoin + "TRNAM ZT2 ON COALESCE(ST.TRCD, ' ') = COALESCE(ZT2.TRCD, ' ') ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    sCmd1 += " LEFT JOIN SS_SDAIHYO DH ON DH.SICD = ST.TRCD AND DH.SIHJCD = ST.HJCD";
                    sCmd1 += " LEFT JOIN TBLSAIKEN SI ON SI.TOKUCD = ST.TRCD AND SI.HJCD = ST.HJCD";
                    if (sSqlWhere != " WHERE ")
                    {
                        sCmd1 += sSqlWhere + " ";
                    }
                }
                if (Global.Prn_PKind == 1 || Global.Prn_PKind == 0)
                {
                    if ((Global.Prn_TRCD_Sta != "") &&
                        (Global.Prn_TRCD_End != ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND COALESCE(ZT.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta != "") &&
                        (Global.Prn_TRCD_End == ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta == "") &&
                        (Global.Prn_TRCD_End != ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta == "") &&
                        (Global.Prn_TRCD_End == ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ";
                    }
                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND ZT.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End;
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ZT.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND ZT.ISTAYMD <> 0) ";
                        }
                        else if (Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND (ZT.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR ZT.ISTAYMD = 0) ";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ZT.ISTAYMD = 0 ";
                    }
                    if (!Global.Prn_ZEDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND ZT.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End;
                        }
                        else if (Global.Prn_ZEDYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ZT.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR ZT.IENDYMD = 0) ";
                        }
                        else if (Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND (ZT.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND ZT.IENDYMD <> 0) ";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ZT.IENDYMD = 0 ";
                    }
                    sCmd2 = "SELECT ZT.TRCD TRCD, null HJCD, ZT.TRMX RYAKU, ZT.TRNAM TORI_NAM, ZT.RNLD KNLD, ";
                    sCmd2 += " '0' TGASW, '1' ZFLG, CASE WHEN ( SELECT COUNT(*) FROM SS_TORI SS WHERE COALESCE(SS.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') AND SS.HJCD = 0 ) > 0 THEN '2' ";
                    sCmd2 += " WHEN ( SELECT COUNT(*) FROM SS_TORI SS WHERE COALESCE(SS.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END SSFLG, ";
                    sCmd2 += " ZT.ISTAYMD ISTAYMD, ZT.IENDYMD IENDYMD, '0' STYMD, '0' EDYMD ";
//-- <2016/03/21>
//                    sCmd2 += " ,CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z1500..TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'Z' TYP FROM ICSP_312Z1500..TRNAM ZT ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sCmd2 += " ,CASE WHEN ( SELECT COUNT(*) FROM ICSP_312Z" + Global.sCcod + "..TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'Z' TYP FROM ICSP_312Z" + Global.sCcod + "..TRNAM ZT ";
                    sCmd2 += " ,CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'Z' TYP FROM "+ Global.sZJoin + "TRNAM ZT ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    sCmd2 += "WHERE " + sSqlWhere + " ";
                }
                if (Global.Prn_PKind == 2 || Global.Prn_PType == 2)
                {
                    Global.cCmdSel.CommandText = sCmd1;
                }
                else if (Global.Prn_PKind == 1)
                {
                    Global.cCmdSel.CommandText = sCmd2;
                }
                else
                {
                    Global.cCmdSel.CommandText = sCmd1 + " UNION ALL " + sCmd2;
                }

                if (Global.Prn_SortKEY == 0)
                {
                    //Global.cCmdSel.CommandText += " ORDER BY VFLG, TRCD, HJCD ";
                    Global.cCmdSel.CommandText += " ORDER BY TRCD, HJCD ";
                }
                else
                {
                    //Global.cCmdSel.CommandText += " ORDER BY KNLD, VFLG, TRCD, HJCD ";
//-- <2016/03/12 カナではなくフリガナ>
//                    Global.cCmdSel.CommandText += " ORDER BY KNLD, TRCD, HJCD ";
                    //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                    if (IcsComUtil.ComUtil.IsPostgreSQL())
                    {
                        Global.cCmdSel.CommandText += " ORDER BY ST.TRFURI NULLS FIRST, TRCD, HJCD ";
                    }
                    else
                    {
                    //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                        Global.cCmdSel.CommandText += " ORDER BY ST.TRFURI, TRCD, HJCD ";
                    }
//-- <2016/03/12>
                }
////                Global.cCmdSel.CommandText = Global.cCmdSel.CommandText.Replace("1500", Global.sCcod);

                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
            #endregion
        }

        /// <summary>
        /// 取引先台帳に出力するデータを取得
        /// </summary>
        private void Sel_TRCD_Info(out int iCnt)
        {
            #region 取引先台帳のデータ取得SQL作成
            iCnt = 0;
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }

                string sSubwhere = " AND T.TRCD = TORI.TRCD ";

                //2013/07/16 ICS.居軒 ▼パフォーマンス改善
                //SELECT句
                Global.cCmdSel.CommandText = " SELECT TORI.TRCD, TORI.HJCD ";
                Global.cCmdSel.CommandText += " , TORI.TORI_NAM, TORI.STFLG, TORI.RYAKU, TORI.KNLD, TORI.TGASW "; /*Header*/
                Global.cCmdSel.CommandText += ", TORI.TRFURI, TORI.SAIKEN, SAIMU, TORI.SAIKEN_FLG, TORI.SAIMU_FLG, TORI.GRPID, GP.GRPNM "; /*Header*/
                Global.cCmdSel.CommandText += ", DH.SIDAICD, DH.SIDAIHJCD, SI.NYDAICD, SI.NYDAIHJCD "; /*Header*/
                if (Global.Prn_Address == 0)
                {
                    Global.cCmdSel.CommandText += " , TORI.HJCD, TORI.ZIP, TORI.ADDR1, TORI.ADDR2, TORI.TEL, TORI.FAX, TORI.SBUSYO ";   /*Grp1*/
                    //Global.cCmdSel.CommandText += " , TORI.SKICD, TORI.STANTO, KEI.KEISNM, TNT.TNAM, BMN.BNAM, KMK.KNAM, KMK.KCOD ";  /*Grp1*/
                    Global.cCmdSel.CommandText += " , TORI.SKICD, TORI.STANTO, KEI.KEISNM, BMN.BNAM, KMK.KNAM, KMK.KCOD ";  /*Grp1*/
                    Global.cCmdSel.CommandText += " , TORI.TRMAIL, TORI.TRURL, TORI.BIKO, TT.TANTOMEI, TORI.CDM1, TORI.IDM1, TORI.MYNO_AITE, TORI.SOSAI, TORI.SRYOU_F ";  /*Grp1*/
                }
                if (Global.Prn_Kaisyu == 0)
                {
                    Global.cCmdSel.CommandText += ", TORI.TOKUKANA, TORI.FUTAN, TORI.KAISYU, TK.KUBUNMEI NYU_KBNMEI, TORI.YAKUJO, TORI.SHIME, TORI.KAISYUHI, TORI.KAISYUSIGHT"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.Y_KINGAKU, TORI.HOLIDAY, TORI.MIMAN, TORI.IJOU_1, TORI.BUNKATSU_1, TORI.HASU_1, TORI.SIGHT_1"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.IJOU_2, TORI.BUNKATSU_2, TORI.HASU_2, TORI.SIGHT_2, TORI.IJOU_3, TORI.BUNKATSU_3, TORI.HASU_3, TORI.SIGHT_3"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.SEN_GINKOCD, BK.BKNAM SEN_BKNAM, TORI.SEN_SITENCD, TORI.SEN_SHITENMEI, TORI.YOKINSYU, TORI.SEN_KOZANO"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.JIDOU_GAKUSYU, TORI.NYUKIN_YOTEI, TORI.TESURYO_GAKUSYU, TORI.TESURYO_GOSA, TORI.RYOSYUSYO"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.SHIN_KAISYACD, TORI.YOSIN, TORI.YOSHINRANK, TORI.GAIKA, TORI.TSUKA"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TORI.GAIKA_KEY_F, TORI.GAIKA_KEY_B, TORI.HIFURIKOZA_1, TORI.HIFURIKOZA_2, TORI.HIFURIKOZA_3"; /*Grp2*/
                    Global.cCmdSel.CommandText += ", OBK1.OWNBKCOD HI_BKCOD1, BK1.BKNAM HI_BKNAM1, OBK1.OWNBRCOD HI_BRCOD1, BR1.BRNAM HI_BRNAM1, OBK1.YOKNKIND HI_YOKINKIND1, OBK1.KOZANO HI_KOZANO1 "; /*Grp2*/
                    Global.cCmdSel.CommandText += ", OBK2.OWNBKCOD HI_BKCOD2, BK2.BKNAM HI_BKNAM2, OBK2.OWNBRCOD HI_BRCOD2, BR2.BRNAM HI_BRNAM2, OBK2.YOKNKIND HI_YOKINKIND2, OBK2.KOZANO HI_KOZANO2 "; /*Grp2*/
                    Global.cCmdSel.CommandText += ", OBK3.OWNBKCOD HI_BKCOD3, BK3.BKNAM HI_BKNAM3, OBK3.OWNBRCOD HI_BRCOD3, BR2.BRNAM HI_BRNAM3, OBK3.YOKNKIND HI_YOKINKIND3, OBK3.KOZANO HI_KOZANO3 "; /*Grp2*/
                    Global.cCmdSel.CommandText += ", TKYM.KUBUNMEI MIMAN_NM, TKY1.KUBUNMEI IJOU_NM1, TKY2.KUBUNMEI IJOU_NM2, TKY3.KUBUNMEI IJOU_NM3 ";
                }
                if (Global.Prn_Others == 0)
                {
                    //Global.cCmdSel.CommandText += " , TORI.NAYOSE, TORI.F_SETUIN, TORI.STAN, O_TNT.TNAM STAN_NM, TORI.SBCOD, TORI.SKICD "; /*Grp6*/
                    Global.cCmdSel.CommandText += " , TORI.NAYOSE, TORI.F_SETUIN, TORI.STAN, TORI.SBCOD, TORI.SKICD "; /*Grp6*/
                    Global.cCmdSel.CommandText += " , O_BMN.BNAM O_BNAM, O_KMK.KNMX O_KNAM, O_KMK.KCOD, TORI.TEGVAL ";
                    Global.cCmdSel.CommandText += " , TORI.F_SOUFU, TORI.ANNAI, TORI.TSOKBN, TORI.SZEI, TORI.DM1, TORI.DM2, TORI.DM3, TORI.F_SHITU ";  /*Grp6*/
                    Global.cCmdSel.CommandText += " , TORI.CDM2, TORI.CD03, TORI.GGKBN, TORI.GSSKBN ";  /*Grp6*/
                    Global.cCmdSel.CommandText += " , TORI.GENSEN, TORI.GOU, TORI.GGKBNM, TORI.GSKUBN, TORI.HORYU, TORI.HOVAL, O_SKBN.SKBNM O_SKBNM ";  /*Grp6*/
                    Global.cCmdSel.CommandText += " , TORI.SZEI, TORI.HR_KIJYUN, TORI.HORYU_F, TORI.HRORYUGAKU, TORI.HRKBN, O_SKBN2.SKBNM O_SKBNM2 "; /*Grp6*/
                }
                if (Global.Prn_Gaika == 0)
                {
//                    Global.cCmdSel.CommandText += " , TORI.HEI_CD, TORI.GAI_SF, TORI.GAI_SH, TORI.GAI_KZID, TORI.GAI_TF, TORI.ENG_NAME, TORI.ENG_ADDR "; /*Grp7*/
                    Global.cCmdSel.CommandText += " , TORI.GAI_F, TORI.HEI_CD, TORI.GAI_SF, TORI.GAI_SH, TORI.GAI_KZID, TORI.GAI_TF, TORI.ENG_NAME, TORI.ENG_ADDR "; /*Grp7*/
                    Global.cCmdSel.CommandText += " , TORI.ENG_KZNO, TORI.ENG_SWIF, TORI.ENG_BNKNAM, TORI.ENG_BRNNAM, TORI.ENG_BNKADDR "; /*Grp7*/
                    Global.cCmdSel.CommandText += " , G_OWN.OWNBKCOD G_OWNBKCOD, G_BK.BKNAM G_BKNAM, G_OWN.OWNBRCOD G_OWNBRCOD, G_BR.BRNAM G_BRNAM, G_OWN.YOKNKIND G_YOKINKIND, G_OWN.KOZANO G_KOZANO, G_OWN.HEI_CD G_HEI_CD ";
                }

                Global.cCmdSel.CommandText += " , TORI.STYMD, TORI.EDYMD, TR.ISTAYMD , TR.IENDYMD ";  /*Grp8*/

                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                //{
                //    Global.cCmdSel.CommandText += " , CASE WHEN LEN(TORI.TRCD) = 13 THEN '1' ELSE '0' END VFLG ";  /*内部FLG*/
                //}
                //else
                //{
                //    Global.cCmdSel.CommandText += " , CASE WHEN LENGTH(TORI.TRCD) = 13 THEN '1' ELSE '0' END VFLG ";  /*内部FLG*/
                //}
                
                //FROM句
                Global.cCmdSel.CommandText += " FROM SS_TORI TORI ";
                Global.cCmdSel.CommandText += " LEFT JOIN SS_SDAIHYO DH ON DH.SICD = TORI.TRCD AND DH.SIHJCD = TORI.HJCD";
                Global.cCmdSel.CommandText += " LEFT JOIN TBLSAIKEN SI ON SI.TOKUCD = TORI.TRCD AND SI.HJCD = TORI.HJCD";
                Global.cCmdSel.CommandText += " LEFT JOIN SS_GROUP GP ON GP.GRPID = TORI.GRPID";
                if (Global.Prn_Address == 0)
                {
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_KEISYO KEI ON TORI.KEICD = KEI.KEICD ";   /*Grp1*/
//-- <2016/03/21>
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..TANTOU TNT ON TORI.STAN = TNT.TCOD ";  /*Grp1*/
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BNAME BMN ON  TORI.SBCOD = BMN.BCOD AND BMN.KESN = " + Global.sKESN.ToString();  /*Grp1*/
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..KNAME KMK ON  TORI.SKICD = KMK.KICD AND KMK.KESN = " + Global.sKESN.ToString() + " AND KMK.BKBN = 5 ";  /*Grp1*/
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..TANTOU TNT ON TORI.STAN = TNT.TCOD ";  /*Grp1*/
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BNAME BMN ON  TORI.SBCOD = BMN.BCOD AND BMN.KESN = " + Global.sKESN.ToString();  /*Grp1*/
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..KNAME KMK ON  TORI.SKICD = KMK.KICD AND KMK.KESN = " + Global.sKESN.ToString() + " AND KMK.BKBN = 5 ";  /*Grp1*/
                    //Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "TANTOU TNT ON TORI.STAN = TNT.TCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BNAME BMN ON  TORI.SBCOD = BMN.BCOD AND BMN.KESN = " + Global.sKESN.ToString();
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "KNAME KMK ON  TORI.SKICD = KMK.KICD AND KMK.KESN = " + Global.sKESN.ToString() + " AND KMK.BKBN = 5 ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLTANTO TT ON TT.TANTOCD = TORI.E_TANTOCD";
                }
                if (Global.Prn_Kaisyu == 0)
                {
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLKUBUN TK ON TK.SIKIBETU = '2' AND TK.KUBUNCD = TORI.KAISYU ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
//-- <2016/03/21>
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BANK BK ON BK.BKCOD = TORI.SEN_GINKOCD ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BK ON BK.BKCOD = TORI.SEN_GINKOCD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BANK BK ON BK.BKCOD = TORI.SEN_GINKOCD ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_OWNBK OBK1 ON OBK1.OWNID = TORI.HIFURIKOZA_1 ";
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_OWNBK OBK2 ON OBK2.OWNID = TORI.HIFURIKOZA_2 ";
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_OWNBK OBK3 ON OBK3.OWNID = TORI.HIFURIKOZA_3 ";
//-- <2016/03/21>
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BANK BK1 ON BK1.BKCOD = OBK1.OWNBKCOD ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BRANCH BR1 ON BR1.BKCOD = OBK1.OWNBKCOD AND BR1.BRCOD = OBK1.OWNBRCOD";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BANK BK2 ON BK2.BKCOD = OBK2.OWNBKCOD ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BRANCH BR2 ON BR2.BKCOD = OBK2.OWNBKCOD AND BR2.BRCOD = OBK2.OWNBRCOD";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BANK BK3 ON BK3.BKCOD = OBK3.OWNBKCOD ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BRANCH BR3 ON BR3.BKCOD = OBK3.OWNBKCOD AND BR3.BRCOD = OBK3.OWNBRCOD";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BK1 ON BK1.BKCOD = OBK1.OWNBKCOD ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BR1 ON BR1.BKCOD = OBK1.OWNBKCOD AND BR1.BRCOD = OBK1.OWNBRCOD";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BK2 ON BK2.BKCOD = OBK2.OWNBKCOD ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BR2 ON BR2.BKCOD = OBK2.OWNBKCOD AND BR2.BRCOD = OBK2.OWNBRCOD";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BK3 ON BK3.BKCOD = OBK3.OWNBKCOD ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BR3 ON BR3.BKCOD = OBK3.OWNBKCOD AND BR3.BRCOD = OBK3.OWNBRCOD";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BANK BK1 ON BK1.BKCOD = OBK1.OWNBKCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BRANCH BR1 ON BR1.BKCOD = OBK1.OWNBKCOD AND BR1.BRCOD = OBK1.OWNBRCOD";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BANK BK2 ON BK2.BKCOD = OBK2.OWNBKCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BRANCH BR2 ON BR2.BKCOD = OBK2.OWNBKCOD AND BR2.BRCOD = OBK2.OWNBRCOD";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BANK BK3 ON BK3.BKCOD = OBK3.OWNBKCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + "BRANCH BR3 ON BR3.BKCOD = OBK3.OWNBKCOD AND BR3.BRCOD = OBK3.OWNBRCOD";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLKUBUN TKYM ON TKYM.SIKIBETU = '2' AND TKYM.KUBUNCD = TORI.MIMAN ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLKUBUN TKY1 ON TKY1.SIKIBETU = '2' AND TKY1.KUBUNCD = TORI.IJOU_1 ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLKUBUN TKY2 ON TKY2.SIKIBETU = '2' AND TKY2.KUBUNCD = TORI.IJOU_2 ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                    Global.cCmdSel.CommandText += " LEFT JOIN TBLKUBUN TKY3 ON TKY3.SIKIBETU = '2' AND TKY3.KUBUNCD = TORI.IJOU_3 ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                }
                if (Global.Prn_Others == 0)
                {
//-- <2016/03/21>
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..TANTOU O_TNT ON TORI.STAN = O_TNT.TCOD ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BNAME O_BMN ON  TORI.SBCOD = O_BMN.BCOD AND O_BMN.KESN = :p ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..KNAME O_KMK ON  TORI.SKICD = O_KMK.KICD AND O_KMK.KESN = :p AND O_KMK.BKBN = 5 ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..TANTOU O_TNT ON TORI.STAN = O_TNT.TCOD ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BNAME O_BMN ON  TORI.SBCOD = O_BMN.BCOD AND O_BMN.KESN = :p ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..KNAME O_KMK ON  TORI.SKICD = O_KMK.KICD AND O_KMK.KESN = :p AND O_KMK.BKBN = 5 ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " TANTOU O_TNT ON TORI.STAN = O_TNT.TCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " BNAME O_BMN ON  TORI.SBCOD = O_BMN.BCOD AND O_BMN.KESN = :p ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " KNAME O_KMK ON  TORI.SKICD = O_KMK.KICD AND O_KMK.KESN = :p AND O_KMK.BKBN = 5 ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_SKUBN O_SKBN ON O_SKBN.SKKBN = 11 AND TORI.GSKUBN = O_SKBN.SKBNCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_SKUBN O_SKBN2 ON O_SKBN2.SKKBN = 11 AND TORI.HRKBN = O_SKBN2.SKBNCOD ";
                }
                if (Global.Prn_Gaika == 0)
                {
                    Global.cCmdSel.CommandText += " LEFT JOIN SS_OWNBK G_OWN ON G_OWN.OWNID = TORI.GAI_KZID ";
//-- <2016/03/21>
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BANK G_BK ON G_BK.BKCOD = G_OWN.OWNBKCOD ";
//                    Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..BRANCH G_BR ON G_BR.BKCOD = G_OWN.OWNBKCOD AND G_BR.BRCOD = G_OWN.OWNBRCOD";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK G_BK ON G_BK.BKCOD = G_OWN.OWNBKCOD ";
                    //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH G_BR ON G_BR.BKCOD = G_OWN.OWNBKCOD AND G_BR.BRCOD = G_OWN.OWNBRCOD";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " BANK G_BK ON G_BK.BKCOD = G_OWN.OWNBKCOD ";
                    Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " BRANCH G_BR ON G_BR.BKCOD = G_OWN.OWNBKCOD AND G_BR.BRCOD = G_OWN.OWNBRCOD";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                }
//-- <2016/03/21>
//                Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z1500..TRNAM TR ON TORI.TRCD = TR.TRCD ";  /*Grp7*/
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //Global.cCmdSel.CommandText += " LEFT JOIN ICSP_312Z" + Global.sCcod + "..TRNAM TR ON TORI.TRCD = TR.TRCD ";  /*Grp7*/
                Global.cCmdSel.CommandText += " LEFT JOIN " + Global.sZJoin + " TRNAM TR ON TORI.TRCD = TR.TRCD ";
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                //WHERE句
                string sWhere = "";
                string sWhere1 = "";
                string sWhere2 = "";

                sWhere = " WHERE ";

                if (Global.Prn_PType == 1 || Global.Prn_PType == 0)
                {
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(IcsComUtil.ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sWhere1 = " ( LENGTH(TORI.TRCD) < 13 ";
                    }
                    else
                    {
                        sWhere1 = " ( LEN(TORI.TRCD) < 13 ";
                    }
                    if (Global.Prn_TRCD_Sta != "" && Global.Prn_HJCD_Sta != "")
                    {
                        sWhere1 += " AND (TORI.TRCD > '" + Global.Prn_TRCD_Sta + "' OR (TORI.TRCD = '" + Global.Prn_TRCD_Sta + "' AND TORI.HJCD >= '" + Global.Prn_HJCD_Sta + "')) ";
                    }
                    else if (Global.Prn_TRCD_Sta != "" && Global.Prn_HJCD_Sta == "")
                    {
                        sWhere1 += " AND TORI.TRCD >= '" + Global.Prn_TRCD_Sta + "' ";
                    }
                    if (Global.Prn_TRCD_End != "" && Global.Prn_HJCD_End != "")
                    {
                        sWhere1 += " AND (TORI.TRCD < '" + Global.Prn_TRCD_End + "' OR (TORI.TRCD = '" + Global.Prn_TRCD_End + "' AND TORI.HJCD <= '" + Global.Prn_HJCD_End + "')) ";
                    }
                    else if (Global.Prn_TRCD_End != "" && Global.Prn_HJCD_End == "")
                    {
                        sWhere1 += " AND TORI.TRCD <= '" + Global.Prn_TRCD_End + "' ";
                    }
                    sWhere1 += " ) ";
                }
                if (Global.Prn_PType == 2 || Global.Prn_PType == 0)
                {
                    if (Global.Prn_TRCD_Once != "")
                   {
                        sWhere2 = string.Format("( TORI.TORI_NAM like '%{0}%' AND ", Global.Prn_TRCD_Once);
                    }
                    else
                    {
                        sWhere2 = " ( ";
                    }

                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(IcsComUtil.ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sWhere2 += " LENGTH(TORI.TRCD) = 13 ) ";
                    }
                    else
                    {
                        sWhere2 += " LEN(TORI.TRCD) = 13 ) ";
                    }
                }
                if (Global.Prn_PType == 1)
                {
                    sWhere += sWhere1;
                }
                else if (Global.Prn_PType == 2)
                {
                    sWhere += sWhere2;
                }
                else
                {
                    sWhere += " ( " + sWhere1 + " OR " + sWhere2 + " ) ";
                }
                // 
                if (!Global.Prn_ZSTYMD_Null)
                {
                    if(Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                    {
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    else if (Global.Prn_ZSTYMD_Sta != 0)
                    {
                        //sWhere += " AND TR.ISTAYMD >= " + Global.Prn_STYMD_Sta;
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    else if (Global.Prn_ZSTYMD_End != 0)
                    {
                        //sWhere += " AND TR.ISTAYMD <= " + Global.Prn_STYMD_End;
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                }
                else
                {
                    //sWhere += " AND TR.ISTAYMD = 0 ";
//-- <2016/03/21>
//                    sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z1500..TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sWhere += " AND EXISTS(SELECT ISTAYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                    sWhere += " AND EXISTS(SELECT ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                }
                if (!Global.Prn_ZEDYMD_Null)
                {
                    if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                    {
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    else if (Global.Prn_ZEDYMD_Sta != 0)
                    {
                        //sWhere += " AND TR.IENDYMD >= " + Global.Prn_EDYMD_Sta;
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                    else if (Global.Prn_ZEDYMD_End != 0)
                    {
                        //sWhere += " AND TR.IENDYMD <= " + Global.Prn_EDYMD_End;
//-- <2016/03/21>
//                        sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + ")";
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + ")";
                        sWhere += " AND EXISTS(SELECT IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + ")";
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                    }
                }
                else
                {
                    //sWhere += " AND TR.IENDYMD = 0 ";
//-- <2016/03/21>
//                    sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z1500..TRNAM T WHERE T.IENDYMD = 0  " + sSubwhere + ") ";
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //sWhere += " AND EXISTS(SELECT IENDYMD FROM ICSP_312Z" + Global.sCcod + "..TRNAM T WHERE T.IENDYMD = 0  " + sSubwhere + ") ";
                    sWhere += " AND EXISTS(SELECT IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD = 0  " + sSubwhere + ") ";
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                }
                if (!Global.Prn_STYMD_Null)
                {
                    if (Global.Prn_STYMD_Sta != 0 && Global.Prn_STYMD_End != 0)
                    {
                        sWhere += " AND TORI.STYMD BETWEEN " + Global.Prn_STYMD_Sta + " AND " + Global.Prn_STYMD_End;
                    }
                    else if (Global.Prn_STYMD_Sta != 0)
                    {
                        sWhere += " AND (TORI.STYMD >= " + Global.Prn_STYMD_Sta + " AND TORI.STYMD <> 0)";
                    }
                    else if (Global.Prn_STYMD_End != 0)
                    {
                        sWhere += " AND (TORI.STYMD <= " + Global.Prn_STYMD_End + " OR TORI.STYMD = 0)";
                    }
                }
                else
                {
                    sWhere += " AND TORI.STYMD = 0 ";
                }
                if (!Global.Prn_EDYMD_Null)
                {
                    if (Global.Prn_EDYMD_Sta != 0 && Global.Prn_EDYMD_End != 0)
                    {
//-- <2016/03/10 範囲指定の元が違っている>
//                        sWhere += " AND TORI.STYMD BETWEEN " + Global.Prn_EDYMD_Sta + " AND " + Global.Prn_EDYMD_End;
                        sWhere += " AND TORI.EDYMD BETWEEN " + Global.Prn_EDYMD_Sta + " AND " + Global.Prn_EDYMD_End;
//-- <2016/03/10>
                    }
                    if (Global.Prn_EDYMD_Sta != 0)
                    {
                        sWhere += " AND (TORI.EDYMD >= " + Global.Prn_EDYMD_Sta + " OR TORI.EDYMD = 0)";
                    }
                    if (Global.Prn_EDYMD_End != 0)
                    {
                        sWhere += " AND (TORI.EDYMD <= " + Global.Prn_EDYMD_End + " AND TORI.EDYMD <> 0)";
                    }
                }
                else
                {
                    sWhere += " AND TORI.EDYMD = 0 ";
                }
                if (Global.PrnTarget == 1)
                {
                    sWhere += " AND TORI.SAIKEN = 1 ";
                }
                else if (Global.PrnTarget == 2)
                {
                    sWhere += " AND TORI.SAIMU = 1 ";
                }
                else if (Global.PrnTarget == 3)
                {
                    sWhere += " AND TORI.TGASW >= 1 ";
                }

                // 
                Global.cCmdSel.CommandText += sWhere;
                //ORDER BY 句
                string sOrderby = "";
                sOrderby += " ORDER BY ";
                if (Global.Prn_SortKEY == 0)
                {
                    //sOrderby += " VFLG, TORI.TRCD, TORI.HJCD ";
                    sOrderby += " TORI.TRCD, TORI.HJCD ";
                }
                else
                {
                    //sOrderby += " TORI.KNLD, VFLG, TORI.TRCD, TORI.HJCD ";
//-- <9999>                    
//                    sOrderby += " TORI.KNLD, TORI.TRCD, TORI.HJCD ";
                    //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                    if (IcsComUtil.ComUtil.IsPostgreSQL())
                    {
                        sOrderby += " TORI.TRFURI NULLS FIRST, TORI.TRCD, TORI.HJCD ";
                    }
                    else
                    {
                    //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                        sOrderby += " TORI.TRFURI, TORI.TRCD, TORI.HJCD ";
                    }
//-- <9999>
                
                }
                Global.cCmdSel.CommandText += sOrderby;
////                Global.cCmdSel.CommandText = Global.cCmdSel.CommandText.Replace("1500", Global.sCcod);
                //実行
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN1", Global.sKESN);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN2", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                iCnt = DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader, out Global.dtTORI);
                // 「回収設定」情報取得

                //「振込先銀行」情報取得
                if (Global.Prn_Frigin == 0)
                {
                    Global.cCmdSel.CommandText = " SELECT FRIGIN.TRCD, FRIGIN.HJCD, FRIGIN.GIN_ID, FRIGIN.YOKIN_TYP, FRIGIN.KOUZA, FRIGIN.TESUU, FRIGIN.SOUKIN, FRIGIN.GENDO, "
                               + " FRIGIN.MEIGI, FRIGIN.MEIGIK, FRIGIN.BANK_CD, FRIGIN.SITEN_ID, BNK.BKNAM, BRN.BRNAM "
                               + ", FRIGIN.FDEF, FRIGIN.DDEF, FRIGIN.FTESUID, TID.TESUNAM, FRIGIN.DTESUSW, FRIGIN.DTESU "
                               + " FROM SS_FRIGIN FRIGIN "
                               + " INNER JOIN SS_TORI TORI ON TORI.TRCD = FRIGIN.TRCD AND TORI.HJCD = FRIGIN.HJCD "
//-- <2016/03/21>
//                               + " LEFT JOIN ICSP_312Z1500..BANK BNK ON FRIGIN.BANK_CD = BNK.BKCOD "
//                               + " LEFT JOIN ICSP_312Z1500..BRANCH BRN ON FRIGIN.BANK_CD = BRN.BKCOD AND FRIGIN.SITEN_ID = BRN.BRCOD "
                               //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                               //+ " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BNK ON FRIGIN.BANK_CD = BNK.BKCOD "
                               //+ " LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BRN ON FRIGIN.BANK_CD = BRN.BKCOD AND FRIGIN.SITEN_ID = BRN.BRCOD "
                               + " LEFT JOIN " + Global.sZJoin + "BANK BNK ON FRIGIN.BANK_CD = BNK.BKCOD "
                               + " LEFT JOIN " + Global.sZJoin + "BRANCH BRN ON FRIGIN.BANK_CD = BRN.BKCOD AND FRIGIN.SITEN_ID = BRN.BRCOD "
                               //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                               + " LEFT JOIN SS_TESUID TID ON TID.TESUID = FRIGIN.FTESUID"
                               + sWhere
                               + sOrderby
                               + " , FRIGIN.GIN_ID ";
////                    Global.cCmdSel.CommandText = Global.cCmdSel.CommandText.Replace("1500", Global.sCcod);
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.ExecuteQuery(ref Global.cCmdSel, out Global.dtFRIGIN);
                }

                //「支払条件」情報取得
                if (Global.Prn_Shiharai == 0)
                {
                    Global.cCmdSel.CommandText = "SELECT TSHOH.TRCD, TSHOH.HJCD, TSHOH.BCOD, TSHOH.KICD, TSHOH.SHO_ID,  BMN.BNAM, KMK.KCOD, KMK.KNAM, SHOHO.SHINO, SHOHO.SICOMENT, SHOHO.SIMEBI, "
                               + "SHOHO.SHIHARAIMM, SHOHO.SIHARAIDD, TSHOH.HARAI_H, SHOHO.SKIJITUMM, SHOHO.SKIJITUDD, TSHOH.KIJITU_H, SKBN.SKBNM, SHOHO.V_YAKUJO, KMK.KCOD,"
                               + "SHOHO.YAKUJOA_L, "
                               + "SHOHO.YAKUJOA_M, "
                               + "SHOHO.YAKUJOB_LH, "
                               + "SHOHO.YAKUJOB_H1, SHOHO.YAKUJOB_R1, SHOHO.YAKUJOB_U1, "
                               + "SHOHO.YAKUJOB_H2, SHOHO.YAKUJOB_R2, SHOHO.YAKUJOB_U2, "
                               + "SHOHO.YAKUJOB_H3, SHOHO.YAKUJOB_R3, SHOHO.YAKUJOB_U3 "
                               + ", SKB1.SKBNM SKBNM1, SKB1.SKBKIND SKBKIND1, OBK1.OWNBKCOD OWNBKCOD1, BNK1.BKNAM BKNAM1, OBK1.OWNBRCOD OWNBRCOD1, BRN1.BRNAM BRNAM1, OBK1.YOKNKIND YOKNKIND1, OBK1.KOZANO KOZANO1, FCT1.FACNAM FACNAM1 "
                               + ", SKB2.SKBNM SKBNM2, SKB2.SKBKIND SKBKIND2, OBK2.OWNBKCOD OWNBKCOD2, BNK2.BKNAM BKNAM2, OBK2.OWNBRCOD OWNBRCOD2, BRN2.BRNAM BRNAM2, OBK2.YOKNKIND YOKNKIND2, OBK2.KOZANO KOZANO2, FCT2.FACNAM FACNAM2 "
                               + ", SKB3.SKBNM SKBNM3, SKB3.SKBKIND SKBKIND3, OBK3.OWNBKCOD OWNBKCOD3, BNK3.BKNAM BKNAM3, OBK3.OWNBRCOD OWNBRCOD3, BRN3.BRNAM BRNAM3, OBK3.YOKNKIND YOKNKIND3, OBK3.KOZANO KOZANO3, FCT3.FACNAM FACNAM3 "
                               + ", SKB4.SKBNM SKBNM4, SKB4.SKBKIND SKBKIND4, OBK4.OWNBKCOD OWNBKCOD4, BNK4.BKNAM BKNAM4, OBK4.OWNBRCOD OWNBRCOD4, BRN4.BRNAM BRNAM4, OBK4.YOKNKIND YOKNKIND4, OBK4.KOZANO KOZANO4, FCT4.FACNAM FACNAM4 "
                               + "FROM SS_TSHOH TSHOH "
                               + "INNER JOIN SS_TORI TORI ON TORI.TRCD = TSHOH.TRCD AND TORI.HJCD = TSHOH.HJCD "
//-- <2016/03/21>
//                               + "LEFT JOIN ICSP_312Z1500..BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
//                               + "LEFT JOIN ICSP_312Z1500..KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
                               //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
                               + "LEFT JOIN " + Global.sZJoin + "BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
                               + "LEFT JOIN " + Global.sZJoin + "KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
                               //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                               + "LEFT JOIN SS_SHOHO SHOHO ON TSHOH.SHINO = SHOHO.SHINO "
                               + "LEFT JOIN SS_SKUBN SKBN ON SKBN.SKKBN = 11 AND SHOHO.SKBNCOD = SKBN.SKBNCOD "

                               + "LEFT JOIN SS_SKUBN SKB1 ON SKB1.SKKBN = 11 AND SKB1.SKBNCOD = TSHOH.SI_KUBN1 "
	                           + "LEFT JOIN SS_SKUBN SKB2 ON SKB2.SKKBN = 11 AND SKB2.SKBNCOD = TSHOH.SI_KUBN2 "
	                           + "LEFT JOIN SS_SKUBN SKB3 ON SKB3.SKKBN = 11 AND SKB3.SKBNCOD = TSHOH.SI_KUBN3 "
	                           + "LEFT JOIN SS_SKUBN SKB4 ON SKB4.SKKBN = 11 AND SKB4.SKBNCOD = TSHOH.SI_KUBN4 "
	                           + "LEFT JOIN SS_OWNBK OBK1 ON OBK1.OWNID = TSHOH.OWNID1 "
	                           + "LEFT JOIN SS_OWNBK OBK2 ON OBK2.OWNID = TSHOH.OWNID2 "
	                           + "LEFT JOIN SS_OWNBK OBK3 ON OBK3.OWNID = TSHOH.OWNID3 "
	                           + "LEFT JOIN SS_OWNBK OBK4 ON OBK4.OWNID = TSHOH.OWNID4 "
	                           + "LEFT JOIN SS_FACTER FCT1 ON FCT1.FACID = TSHOH.OWNID1 "
	                           + "LEFT JOIN SS_FACTER FCT2 ON FCT2.FACID = TSHOH.OWNID2 "
	                           + "LEFT JOIN SS_FACTER FCT3 ON FCT3.FACID = TSHOH.OWNID3 "
	                           + "LEFT JOIN SS_FACTER FCT4 ON FCT4.FACID = TSHOH.OWNID4 "
//-- <2016/03/21>
//	                           + "LEFT JOIN ICSP_312Z1500..BANK BNK1 ON BNK1.BKCOD = OBK1.OWNBKCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BANK BNK2 ON BNK2.BKCOD = OBK2.OWNBKCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BANK BNK3 ON BNK3.BKCOD = OBK3.OWNBKCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BANK BNK4 ON BNK4.BKCOD = OBK4.OWNBKCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BRANCH BRN1 ON BRN1.BKCOD = OBK1.OWNBKCOD AND BRN1.BRCOD = OBK1.OWNBRCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BRANCH BRN2 ON BRN2.BKCOD = OBK2.OWNBKCOD AND BRN2.BRCOD = OBK2.OWNBRCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BRANCH BRN3 ON BRN3.BKCOD = OBK3.OWNBKCOD AND BRN3.BRCOD = OBK3.OWNBRCOD "
//	                           + "LEFT JOIN ICSP_312Z1500..BRANCH BRN4 ON BRN4.BKCOD = OBK4.OWNBKCOD AND BRN4.BRCOD = OBK4.OWNBRCOD "
                               //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BNK1 ON BNK1.BKCOD = OBK1.OWNBKCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BNK2 ON BNK2.BKCOD = OBK2.OWNBKCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BNK3 ON BNK3.BKCOD = OBK3.OWNBKCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK BNK4 ON BNK4.BKCOD = OBK4.OWNBKCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BRN1 ON BRN1.BKCOD = OBK1.OWNBKCOD AND BRN1.BRCOD = OBK1.OWNBRCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BRN2 ON BRN2.BKCOD = OBK2.OWNBKCOD AND BRN2.BRCOD = OBK2.OWNBRCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BRN3 ON BRN3.BKCOD = OBK3.OWNBKCOD AND BRN3.BRCOD = OBK3.OWNBRCOD "
                               //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH BRN4 ON BRN4.BKCOD = OBK4.OWNBKCOD AND BRN4.BRCOD = OBK4.OWNBRCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BANK BNK1 ON BNK1.BKCOD = OBK1.OWNBKCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BANK BNK2 ON BNK2.BKCOD = OBK2.OWNBKCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BANK BNK3 ON BNK3.BKCOD = OBK3.OWNBKCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BANK BNK4 ON BNK4.BKCOD = OBK4.OWNBKCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BRANCH BRN1 ON BRN1.BKCOD = OBK1.OWNBKCOD AND BRN1.BRCOD = OBK1.OWNBRCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BRANCH BRN2 ON BRN2.BKCOD = OBK2.OWNBKCOD AND BRN2.BRCOD = OBK2.OWNBRCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BRANCH BRN3 ON BRN3.BKCOD = OBK3.OWNBKCOD AND BRN3.BRCOD = OBK3.OWNBRCOD "
                               + "LEFT JOIN " + Global.sZJoin + "BRANCH BRN4 ON BRN4.BKCOD = OBK4.OWNBKCOD AND BRN4.BRCOD = OBK4.OWNBRCOD "
                               //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
//-- <2016/03/21>
                               + sWhere
                               + sOrderby
                               + " , TSHOH.SHO_ID ";
////                    Global.cCmdSel.CommandText = Global.cCmdSel.CommandText.Replace("1500", Global.sCcod);
                    Global.cCmdSel.Parameters.Clear();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN1", Global.sKESN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN2", Global.sKESN);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.ExecuteQuery(ref Global.cCmdSel, out Global.dtTSHOH_SJ);
                }

                return;

                //2013/07/16 ICS.居軒 ▲パフォーマンス改善

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
            #endregion
        }


        /// <summary>
        /// ヘッダーに出力するデータを取得
        /// </summary>
        private void Sel_Header_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //ヘッダーに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT TRCD, HJCD, TORI_NAM, STFLG, RYAKU, KNLD, NAYOSE, F_SETUIN, TGASW "
                                           + "FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }


        /// <summary>
        /// 住所等に出力するデータを取得
        /// </summary>
        private void Sel_Grp1_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //住所等タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT TORI.TRCD, TORI.HJCD, TORI.ZIP, TORI.STANTO, TORI.ADDR1, TORI.ADDR2, TORI.TEL, TORI.FAX, TORI.SBUSYO, "
                                           //+ "TORI.SKICD, KEI.KEISNM, TNT.TNAM, BMN.BNAM, KMK.KNAM, KMK.KCOD "
                                           + "TORI.SKICD, KEI.KEISNM, BMN.BNAM, KMK.KNAM, KMK.KCOD "
                                           + "FROM SS_TORI TORI "
                                           + "LEFT JOIN SS_KEISYO KEI ON TORI.KEICD = KEI.KEICD "
                                           //+ "LEFT JOIN TANTOU TNT ON TORI.STAN = TNT.TCOD "
                                           + "LEFT JOIN BNAME BMN ON  TORI.SBCOD = BMN.BCOD AND BMN.KESN = :p "
                                           + "LEFT JOIN KNAME KMK ON  TORI.SKICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
                                           + "WHERE RTRIM(TORI.TRCD) = :p AND TORI.HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }


        /// <summary>
        /// 振込先銀行に出力するデータを取得
        /// </summary>
        private int Sel_Grp2_Info_Pre(string sTRCD, string sHJCD)
        {
            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //Global.drFRIGIN = Global.dtFRIGIN.Select("TRCD = '" + sTRCD + "' AND HJCD = '" + sHJCD + "'");
            Global.drFRIGIN = Global.dtFRIGIN.Select("TRCD = '" + sTRCD + "' AND HJCD = " + int.Parse(sHJCD));
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
            int nCnt = Global.drFRIGIN.Count();
            return nCnt;
            //int nCnt = 0;
            //try
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //    //振込先銀行タブに設定する情報の検索SQL生成&実行
            //    Global.cCmdSel.CommandText = "SELECT COUNT(0) AS nCnt FROM SS_FRIGIN FRIGIN "
            //                               + "LEFT JOIN BANK BNK ON FRIGIN.BANK_CD = BNK.BKCOD "
            //                               + "LEFT JOIN BRANCH BRN ON FRIGIN.BANK_CD = BRN.BKCOD AND FRIGIN.SITEN_ID = BRN.BRCOD "
            //                               + "WHERE FRIGIN.TRCD = :p AND FRIGIN.HJCD = :p ";
            //    Global.cCmdSel.Parameters.Clear();
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
            //    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

            //    if (Global.gcDataReader.HasRows == true)
            //    {
            //        nCnt = DbCls.GetNumNullZero<int>(Global.gcDataReader["nCnt"]);
            //    }
            //    return nCnt;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
            //        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return nCnt;
            //}
            //finally
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //}
            //2013/07/16 ICS.居軒 ▲パフォーマンス改善
        }

        
        /// <summary>
        /// 振込先銀行に出力するデータを取得
        /// </summary>
        private void Sel_Grp2_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //振込先銀行タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT FRIGIN.GIN_ID, FRIGIN.YOKIN_TYP, FRIGIN.KOUZA, FRIGIN.TESUU, FRIGIN.SOUKIN, FRIGIN.GENDO, "
                                           + "FRIGIN.MEIGI, FRIGIN.MEIGIK, FRIGIN.BANK_CD, FRIGIN.SITEN_ID, BNK.BKNAM, BRN.BRNAM "
                                           + "FROM SS_FRIGIN FRIGIN "
                                           + "LEFT JOIN BANK BNK ON FRIGIN.BANK_CD = BNK.BKCOD "
                                           + "LEFT JOIN BRANCH BRN ON FRIGIN.BANK_CD = BRN.BKCOD AND FRIGIN.SITEN_ID = BRN.BRCOD "
                                           + "WHERE RTRIM(FRIGIN.TRCD) = :p AND FRIGIN.HJCD = :p  ORDER BY FRIGIN.GIN_ID ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }


        /// <summary>
        /// 支払方法に出力するデータを取得
        /// </summary>
        private int Sel_Grp3_Info_Pre(string sTRCD, string sHJCD)
        {
            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //Global.drTSHOH_SJ = Global.dtTSHOH_SJ.Select("TRCD = '" + sTRCD + "' AND HJCD = '" + sHJCD + "'");
            Global.drTSHOH_SJ = Global.dtTSHOH_SJ.Select("TRCD = '" + sTRCD + "' AND HJCD = " + int.Parse(sHJCD));
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
            int nCnt = Global.drTSHOH_SJ.Count();
            return nCnt;
            //int nCnt = 0;
            //try
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //    //支払方法タブに設定する情報の検索SQL生成&実行
            //    Global.cCmdSel.CommandText = "SELECT COUNT(0) AS nCnt FROM SS_TSHOH TSHOH "
            //                               + "LEFT JOIN BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
            //                               + "LEFT JOIN KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
            //                               + "LEFT JOIN SS_SHOHO SHOHO ON TSHOH.SHINO = SHOHO.SHINO "
            //                               + "LEFT JOIN SS_SKUBN SKBN ON SHOHO.SKBNCOD = SKBN.SKBNCOD "
            //                               + "WHERE TSHOH.TRCD = :p AND TSHOH.HJCD = :p ";
            //    Global.cCmdSel.Parameters.Clear();
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
            //    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

            //    if (Global.gcDataReader.HasRows == true)
            //    {
            //        nCnt = DbCls.GetNumNullZero<int>(Global.gcDataReader["nCnt"]);
            //    }
            //    return nCnt;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
            //        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return nCnt;
            //}
            //finally
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //}
            //2013/07/16 ICS.居軒 ▲パフォーマンス改善
        }


        /// <summary>
        /// 支払方法に出力するデータを取得
        /// </summary>
        private void Sel_Grp3_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //支払方法タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT TSHOH.BCOD, TSHOH.KICD, TSHOH.SHO_ID,  BMN.BNAM, KMK.KCOD, KMK.KNAM, SHOHO.SHINO, SHOHO.SICOMENT, SHOHO.SIMEBI, "
                                           + "SHOHO.SHIHARAIMM, SHOHO.SIHARAIDD, TSHOH.HARAI_H, SHOHO.SKIJITUMM, SHOHO.SKIJITUDD, TSHOH.KIJITU_H, SKBN.SKBNM, SHOHO.V_YAKUJO, KMK.KCOD,"
                                           + "SHOHO.YAKUJOA_L, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOA_L = SKBNCOD) AS SKBNM1, "
                                           + "SHOHO.YAKUJOA_M, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOA_M = SKBNCOD) AS SKBNM2, "
                                           + "SHOHO.YAKUJOB_LH, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOB_LH = SKBNCOD) AS SKBNM3, "
                                           + "SHOHO.YAKUJOB_H1, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOB_H1 = SKBNCOD) AS SKBNM4, SHOHO.YAKUJOB_R1, SHOHO.YAKUJOB_U1, SHOHO.YAKUJOB_S1, "
                                           + "SHOHO.YAKUJOB_H2, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOB_H2 = SKBNCOD) AS SKBNM5, SHOHO.YAKUJOB_R2, SHOHO.YAKUJOB_U2, SHOHO.YAKUJOB_S2, "
                                           + "SHOHO.YAKUJOB_H3, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SHOHO.YAKUJOB_H3 = SKBNCOD) AS SKBNM6, SHOHO.YAKUJOB_R3, SHOHO.YAKUJOB_U3, SHOHO.YAKUJOB_S3 "
                                           + "FROM SS_TSHOH TSHOH "
                                           + "LEFT JOIN BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
                                           + "LEFT JOIN KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5 "
                                           + "LEFT JOIN SS_SHOHO SHOHO ON TSHOH.SHINO = SHOHO.SHINO "
                                           + "LEFT JOIN SS_SKUBN SKBN ON SKKBN = 11 AND SHOHO.SKBNCOD = SKBN.SKBNCOD "
//**>>
//**                                           + "WHERE TSHOH.TRCD = :p AND TSHOH.HJCD = :p ";
                                           + "WHERE RTRIM(TSHOH.TRCD) = :p AND TSHOH.HJCD = :p "//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + " ORDER BY TSHOH.SHO_ID";
//**<<
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@b.KESN", Global.sKESN);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@c.KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@b.KESN",DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@c.KESN",DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }


        /// <summary>
        /// 自社銀行等に出力するデータを取得
        /// </summary>
        private int Sel_Grp4_Info_Pre(string sTRCD, string sHJCD)
        {
            //2013/07/16 ICS.居軒 ▼パフォーマンス改善
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //Global.drTSHOH_JG = Global.dtTSHOH_JG.Select("TRCD = '" + sTRCD + "' AND HJCD = '" + sHJCD + "'");
            Global.drTSHOH_JG = Global.dtTSHOH_JG.Select("TRCD = '" + sTRCD + "' AND HJCD = " + int.Parse(sHJCD));
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
            int nCnt = Global.drTSHOH_JG.Count();
            return nCnt;
            //int nCnt = 0;
            //try
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //    //自社銀行等タブに設定する情報の検索SQL生成&実行
            //    Global.cCmdSel.CommandText = "SELECT COUNT(0) AS nCnt FROM SS_TSHOH TSHOH "
            //                               + "LEFT JOIN BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
            //                               + "LEFT JOIN KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5"
            //                               + "LEFT JOIN SS_SHOHO SHOHO ON TSHOH.SHINO = SHOHO.SHINO "
            //                               + "WHERE TSHOH.TRCD = :p AND TSHOH.HJCD = :p ";
            //    Global.cCmdSel.Parameters.Clear();
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
            //    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
            //    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

            //    if (Global.gcDataReader.HasRows == true)
            //    {
            //        nCnt = DbCls.GetNumNullZero<int>(Global.gcDataReader["nCnt"]);
            //    }
            //    return nCnt;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
            //        Global.sPrgName +"　Ver"+Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return nCnt;
            //}
            //finally
            //{
            //    if (Global.gcDataReader != null)
            //    {
            //        Global.gcDataReader.Close();
            //        Global.gcDataReader.Dispose();
            //    }
            //}
            //2013/07/16 ICS.居軒 ▲パフォーマンス改善
        }


        /// <summary>
        /// 自社銀行等に出力するデータを取得
        /// </summary>
        private void Sel_Grp4_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //自社銀行等タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT TSHOH.BCOD, TSHOH.KICD, TSHOH.SHO_ID,  BMN.BNAM, KMK.KNAM, KMK.KCOD, SHOHO.SHINO, SHOHO.SICOMENT, "
                                           + "TSHOH.SI_KUBN1, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = TSHOH.SI_KUBN1) AS SKBNM1, (SELECT SKBKIND FROM SS_SKUBN WHERE SKBNCOD = TSHOH.SI_KUBN1) AS SKBKIND1, TSHOH.SI_BANK1, (SELECT BKNAM FROM BANK WHERE BKCOD = TSHOH.SI_BANK1) AS BKNAM1, "
                                           + "TSHOH.SI_SITEN1, (SELECT BRNAM FROM BRANCH WHERE BKCOD = TSHOH.SI_BANK1 AND BRCOD = TSHOH.SI_SITEN1) AS BRNAM1, TSHOH.SI_KOZA1, TSHOH.SI_KOZANO1, TSHOH.SI_IRAININ1, "
                                           + "(SELECT FACNAM FROM SS_OWNBK WHERE OWNBKCOD = TSHOH.SI_BANK1 AND OWNBRCOD = TSHOH.SI_SITEN1 AND YOKNKIND = TSHOH.SI_KOZA1 AND KOZANO = TSHOH.SI_KOZANO1 AND IRAININ = TSHOH.SI_IRAININ1) AS FACNAM1, "
                                           + "TSHOH.SI_KUBN2, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = TSHOH.SI_KUBN2) AS SKBNM2, (SELECT SKBKIND FROM SS_SKUBN WHERE SKBNCOD = TSHOH.SI_KUBN2) AS SKBKIND2, TSHOH.SI_BANK2, (SELECT BKNAM FROM BANK WHERE BKCOD = TSHOH.SI_BANK2) AS BKNAM2, "
                                           + "TSHOH.SI_SITEN2, (SELECT BRNAM FROM BRANCH WHERE BKCOD = TSHOH.SI_BANK2 AND BRCOD = TSHOH.SI_SITEN2) AS BRNAM2, TSHOH.SI_KOZA2, TSHOH.SI_KOZANO2, TSHOH.SI_IRAININ2, "
                                           + "(SELECT FACNAM FROM SS_OWNBK WHERE OWNBKCOD = TSHOH.SI_BANK2 AND OWNBRCOD = TSHOH.SI_SITEN2 AND YOKNKIND = TSHOH.SI_KOZA2 AND KOZANO = TSHOH.SI_KOZANO2 AND IRAININ = TSHOH.SI_IRAININ2) AS FACNAM2, "
                                           + "TSHOH.SI_KUBN3, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = TSHOH.SI_KUBN3) AS SKBNM3, (SELECT SKBKIND FROM SS_SKUBN WHERE SKBNCOD = TSHOH.SI_KUBN3) AS SKBKIND3, TSHOH.SI_BANK3, (SELECT BKNAM FROM BANK WHERE BKCOD = TSHOH.SI_BANK3) AS BKNAM3, "
                                           + "TSHOH.SI_SITEN3, (SELECT BRNAM FROM BRANCH WHERE BKCOD = TSHOH.SI_BANK3 AND BRCOD = TSHOH.SI_SITEN3) AS BRNAM3, TSHOH.SI_KOZA3, TSHOH.SI_KOZANO3, TSHOH.SI_IRAININ3, "
                                           + "(SELECT FACNAM FROM SS_OWNBK WHERE OWNBKCOD = TSHOH.SI_BANK3 AND OWNBRCOD = TSHOH.SI_SITEN3 AND YOKNKIND = TSHOH.SI_KOZA3 AND KOZANO = TSHOH.SI_KOZANO3 AND IRAININ = TSHOH.SI_IRAININ3) AS FACNAM3, "
                                           + "TSHOH.SI_KUBN4, (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = TSHOH.SI_KUBN4) AS SKBNM4, (SELECT SKBKIND FROM SS_SKUBN WHERE SKBNCOD = TSHOH.SI_KUBN4) AS SKBKIND4, TSHOH.SI_BANK4, (SELECT BKNAM FROM BANK WHERE BKCOD = TSHOH.SI_BANK4) AS BKNAM4, "
                                           + "TSHOH.SI_SITEN4, (SELECT BRNAM FROM BRANCH WHERE BKCOD = TSHOH.SI_BANK4 AND BRCOD = TSHOH.SI_SITEN4) AS BRNAM4, TSHOH.SI_KOZA4, TSHOH.SI_KOZANO4, TSHOH.SI_IRAININ4, "
                                           + "(SELECT FACNAM FROM SS_OWNBK WHERE OWNBKCOD = TSHOH.SI_BANK4 AND OWNBRCOD = TSHOH.SI_SITEN4 AND YOKNKIND = TSHOH.SI_KOZA4 AND KOZANO = TSHOH.SI_KOZANO4 AND IRAININ = TSHOH.SI_IRAININ4) AS FACNAM4 "
                                           + "FROM SS_TSHOH TSHOH "
                                           + "LEFT JOIN BNAME BMN ON TSHOH.BCOD = BMN.BCOD AND BMN.KESN = :p "
                                           + "LEFT JOIN KNAME KMK ON TSHOH.KICD = KMK.KICD AND KMK.KESN = :p AND KMK.BKBN = 5"
                                           + "LEFT JOIN SS_SHOHO SHOHO ON TSHOH.SHINO = SHOHO.SHINO "
//**>>
//**                                           + "WHERE TSHOH.TRCD = :p AND TSHOH.HJCD = :p ";
                                           +"WHERE RTRIM(TSHOH.TRCD) = :p AND TSHOH.HJCD = :p "//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           +" ORDER BY TSHOH.SHO_ID";
//**<<
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/12>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }

       
        /// <summary>
        /// 控除に出力するデータを取得
        /// </summary>
        private void Sel_Grp5_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //控除タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT TORI.GENSEN, TORI.GOU, TORI.GGKBNM, TORI.GSKUBN, TORI.HORYU, TORI.HOVAL, SKBN.SKBNM "
                                           + "FROM SS_TORI TORI "
                                           + "LEFT JOIN SS_SKUBN SKBN ON SKBN.SKKBN = 11 AND TORI.GSKUBN = SKBN.SKBNCOD "
                                           + "WHERE RTRIM(TORI.TRCD) = :p AND TORI.HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }


        /// <summary>
        /// その他に出力するデータを取得
        /// </summary>
        private void Sel_Grp6_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //その他情報タブに設定する情報の検索SQL生成&実行
                //**>>ICS-S 2013/05/20
                //**//**>>ICS-S 2013/05/17
                //**//**Global.cCmdSel.CommandText = "SELECT F_SOUFU, ANNAI, TSOKBN, SZEI, DM1, DM2, DM3, F_SHITU FROM SS_TORI WHERE TRCD = :p AND HJCD = :p ";
                //**Global.cCmdSel.CommandText = "SELECT F_SOUFU, ANNAI, TSOKBN, SZEI, DM1, DM2, DM3, F_SHITU, CDM1 FROM SS_TORI WHERE TRCD = :p AND HJCD = :p ";
                Global.cCmdSel.CommandText = "SELECT F_SOUFU, ANNAI, TSOKBN, SZEI, DM1, DM2, DM3, F_SHITU, CDM1, CDM2, IDM1, CD03 FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                //**//**<<ICS-E
                //**<<ICS-E
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }

        
        /// <summary>
        /// ﾏｽﾀｰ情報に出力するデータを取得
        /// </summary>
        private void Sel_Grp7_Info(string sTRCD, string sHJCD)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                //ﾏｽﾀｰ情報タブに設定する情報の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT STYMD, EDYMD FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/21>
            }
        }

        //2013/07/16 ICS.居軒 ▼パフォーマンス改善

        private Dictionary<short, string> dctSKBNM = new Dictionary<short, string>();
        private Dictionary<short, short> dctSKBKIND = new Dictionary<short, short>();
        private Dictionary<string, string> dctBKNAM = new Dictionary<string, string>();
        private Dictionary<string, string> dctBRNAM = new Dictionary<string, string>();
        private Dictionary<string, string> dctFACNAM = new Dictionary<string, string>();

        private string GetSKBNM(object oSkbncod)
        {
            Int16 nSkbncod = DbCls.GetNumNullZero<Int16>(oSkbncod);
            string sRet = "";
            if (nSkbncod > 0)
            {
                if (dctSKBNM.ContainsKey(nSkbncod) == true)
                {
                    sRet = dctSKBNM[nSkbncod];
                }
                else
                {
                    System.Data.Common.DbCommand comm = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    System.Data.DataTable DT = null;
                    comm.CommandText = "SELECT SKBNCOD, SKBNM, SKBKIND FROM SS_SKUBN "
                                               + "WHERE SKKBN = 11 AND SKBNCOD = :p ";
                    comm.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref comm, "@SKBNCOD", nSkbncod);
                    if (DbCls.ExecuteQuery(ref comm, out DT) > 0)
                    {
                        dctSKBNM.Add(nSkbncod, DbCls.GetStrNullKara(DT.Rows[0]["SKBNM"]));
                        dctSKBKIND.Add(nSkbncod, DbCls.GetNumNullZero<Int16>(DT.Rows[0]["SKBKIND"]));
                        sRet = dctSKBNM[nSkbncod];
                    }
                    DT.Clear();
                    DT.Dispose();
                    DT = null;
                    comm.Dispose();
                    comm = null;
                }
            }
            return sRet;
        }

        private Int16 GetSKBKIND(object oSkbncod)
        {
            Int16 nSkbncod = DbCls.GetNumNullZero<Int16>(oSkbncod);
            Int16 nRet = 0;
            if (nSkbncod > 0)
            {
                if (dctSKBKIND.ContainsKey(nSkbncod) == true)
                {
                    nRet = dctSKBKIND[nSkbncod];
                }
                else
                {
                    System.Data.Common.DbCommand comm = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    System.Data.DataTable DT = null;
                    comm.CommandText = "SELECT SKBNCOD, SKBNM, SKBKIND FROM SS_SKUBN "
                                               + "WHERE SKKBN = 11 AND SKBNCOD = :p ";
                    comm.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref comm, "@SKBNCOD", nSkbncod);
                    if (DbCls.ExecuteQuery(ref comm, out DT) > 0)
                    {
                        dctSKBNM.Add(nSkbncod, DbCls.GetStrNullKara(DT.Rows[0]["SKBNM"]));
                        dctSKBKIND.Add(nSkbncod, DbCls.GetNumNullZero<Int16>(DT.Rows[0]["SKBKIND"]));
                        nRet = dctSKBKIND[nSkbncod];
                    }
                    DT.Clear();
                    DT.Dispose();
                    DT = null;
                    comm.Dispose();
                    comm = null;
                }
            }
            return nRet;
        }

        private string GetBKNAM(object oBKCOD)
        {
            string sBKCOD = DbCls.GetStrNullKara(oBKCOD);
            string sRet = "";
            if (sBKCOD != "")
            {
                if (dctBKNAM.ContainsKey(sBKCOD) == true)
                {
                    sRet = dctBKNAM[sBKCOD];
                }
                else
                {
                    System.Data.Common.DbCommand comm = DbCls.CreateCommandObject(ref Global.cConKaisya);
                    System.Data.DataTable DT = null;
                    comm.CommandText = "SELECT BKNAM FROM BANK "
                                               + "WHERE BKCOD = :p ";
                    comm.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref comm, "@BKCOD", sBKCOD);
                    if (DbCls.ExecuteQuery(ref comm, out DT) > 0)
                    {
                        dctBKNAM.Add(sBKCOD, DbCls.GetStrNullKara(DT.Rows[0]["BKNAM"]));
                        sRet = dctBKNAM[sBKCOD];
                    }
                    DT.Clear();
                    DT.Dispose();
                    DT = null;
                    comm.Dispose();
                    comm = null;
                }
            }
            return sRet;
        }

        private string GetBRNAM(object oBKCOD, object oBRCOD)
        {
            string sBKCOD = DbCls.GetStrNullKara(oBKCOD);
            string sBRCOD = DbCls.GetStrNullKara(oBRCOD);
            string sRet = "";
            string sKey = "";
            if (sBKCOD != "" && sBRCOD != "")
            {
                sKey = sBKCOD + @"\" + sBRCOD;
                if (dctBRNAM.ContainsKey(sKey) == true)
                {
                    sRet = dctBRNAM[sKey];
                }
                else
                {
                    System.Data.Common.DbCommand comm = DbCls.CreateCommandObject(ref Global.cConKaisya);
                    System.Data.DataTable DT = null;
                    comm.CommandText = "SELECT BRNAM FROM BRANCH "
                                               + "WHERE BKCOD = :p AND BRCOD = :p ";
                    comm.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref comm, "@BKCOD", sBKCOD);
                    DbCls.AddParamaterByValue(ref comm, "@BRCOD", sBRCOD);
                    if (DbCls.ExecuteQuery(ref comm, out DT) > 0)
                    {
                        dctBRNAM.Add(sKey, DbCls.GetStrNullKara(DT.Rows[0]["BRNAM"]));
                        sRet = dctBRNAM[sKey];
                    }
                    DT.Clear();
                    DT.Dispose();
                    DT = null;
                    comm.Dispose();
                    comm = null;
                }
            }
            return sRet;
        }

        private string GetFACNAM(object oOWNBKCOD, object oOWNBRCOD, object oYOKNKIND, object oKOZANO, object oIRAININ)
        {
            string sOWNBKCOD = DbCls.GetStrNullKara(oOWNBKCOD);
            string sOWNBRCOD = DbCls.GetStrNullKara(oOWNBRCOD);
            string sYOKNKIND = DbCls.GetStrNullKara(oYOKNKIND);
            string sKOZANO = DbCls.GetStrNullKara(oKOZANO);
            string sIRAININ = DbCls.GetStrNullKara(oIRAININ);
            string sRet = "";
            string sKey = "";
            if (sOWNBKCOD != "" && sOWNBRCOD != "" && sYOKNKIND != "" && sKOZANO != "" && sIRAININ != "")
            {
                sKey = sOWNBKCOD + @"\" + sOWNBRCOD + @"\" + sYOKNKIND + @"\" + sKOZANO + @"\" + sIRAININ;
                if (dctFACNAM.ContainsKey(sKey) == true)
                {
                    sRet = dctFACNAM[sKey];
                }
                else
                {
                    System.Data.Common.DbCommand comm = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    System.Data.DataTable DT = null;
                    comm.CommandText = "SELECT FACNAM FROM SS_OWNBK "
                                               + "WHERE OWNBKCOD = :p AND OWNBRCOD = :p AND YOKNKIND = :p AND KOZANO = :p AND IRAININ = :p ";
                    comm.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref comm, "@OWNBKCOD", sOWNBKCOD);
                    DbCls.AddParamaterByValue(ref comm, "@OWNBRCOD", sOWNBRCOD);
                    DbCls.AddParamaterByValue(ref comm, "@YOKNKIND", sYOKNKIND);
                    DbCls.AddParamaterByValue(ref comm, "@KOZANO", sKOZANO);
                    DbCls.AddParamaterByValue(ref comm, "@IRAININ", sIRAININ);
                    if (DbCls.ExecuteQuery(ref comm, out DT) > 0)
                    {
                        dctFACNAM.Add(sKey, DbCls.GetStrNullKara(DT.Rows[0]["FACNAM"]));
                        sRet = dctFACNAM[sKey];
                    }
                    DT.Clear();
                    DT.Dispose();
                    DT = null;
                    comm.Dispose();
                    comm = null;
                }
            }
            return sRet;
        }

        //2013/07/16 ICS.居軒 ▲パフォーマンス改善

        public string StringCut(string pStr, int pLen)
        {
            byte[] bData;
            byte[] boData = new byte[pLen];
            string sData;

            bData = Encoding.GetEncoding(932).GetBytes(pStr);
            if (bData.Length <= pLen)
            {
                return pStr;
            }
            for (int i = 0; i < pLen; i++)
            {
                boData[i] = bData[i];
            }
            sData = Encoding.GetEncoding(932).GetString(boData);
            if (Encoding.GetEncoding(932).GetByteCount(sData) != pLen)
            {
                boData[pLen - 1] = 0x20;
                sData = System.Text.Encoding.GetEncoding(932).GetString(boData);
            }

            return sData;
        }

        public string Get_Tesuu_NM(int pTesuu)
        {
            switch (pTesuu)
            {
                case 1:
                    return "自社負担";
                case 2:
                    return "客先負担";
                case 3:
                    return "折半";
                case 4:
                    return "一律";
            }
            return "";
        }

        public string Get_YokinType_NM(string pYokinType)
        {
            switch (pYokinType)
            {
                case "1":
                    return "普通";
                case "2":
                    return "当座";
                case "4":
                    return "貯蓄";
                case "9":
                    return "他　 ";
            }
            return "";
        }

    }
}
