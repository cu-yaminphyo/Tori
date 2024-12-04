using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Data.Common;

using IcsComInfo;
using IcsComDb;
using System.Windows.Forms;

//-- 
using System.Collections;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace SMTORI
{
    class Global
    {
        //◆Public変数◆
        internal static string[] sArgArray = null;  // コマンドライン引数・文字列配列
        internal static string sPrgId = "";     // プログラムID
        internal static string sPrgName = "";   // 業務名称
        internal static string sPrgVer = "";    // バージョン

        internal static string sCcod = "";  // 会社コード
        internal static int nUcod = 0;      // ユーザID
        internal static int nDebugFlg = 0;
        internal static string sKESN = "";
        internal static string sTRCD_R = "";
        internal static string sHJCD_R = "";
        internal static string sTRNAM_R = "";
        internal static int nShTgSW = 0;
        internal static bool bZMode = false;

        //**>>
        internal static int nTSHOH_cnt_OLD = 0;
        internal static int nTSHOH_cnt = 0;
        internal static int nFRGIN_cnt_OLD = 0;
        internal static int nFRGIN_cnt = 0;
        //**<<

        internal static string sExportPath = "";
        internal static string sExpImpPath = "";    // パス名
        internal static string sExpImpFileNm = "";  // ファイル名
        internal static int nComTitle = 0;
        internal static int nComCD = 0;
        internal static string sComment = "";
        internal static string sZJoin = "";//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】

        internal static System.Data.Common.DbConnection cConSaikenSaimu;        //接続オブジェクト(会社DB（ＳＳ）)
        internal static System.Data.Common.DbConnection cConKaisya;             //接続オブジェクト(会社DB)
        internal static System.Data.Common.DbConnection cConCommon;             // 接続オブジェクト(共通DB)
        internal static System.Data.Common.DbDataReader gcDataReader = null;    //データリーダー

        //2013/07/16 ICS.居軒 ▼パフォーマンス改善
        internal static System.Data.DataTable dtTORI = null;                //DataTable（取引先本体用）
        internal static System.Data.DataTable dtFRIGIN = null;              //DataTable（振込先銀行用）
        internal static System.Data.DataTable dtTSHOH_SJ = null;            //DataTable（支払条件用）
        internal static System.Data.DataTable dtTSHOH_JG = null;            //DataTable（自社銀行用）
        internal static System.Data.DataRow[] drFRIGIN = null;
        internal static System.Data.DataRow[] drTSHOH_SJ = null;
        internal static System.Data.DataRow[] drTSHOH_JG = null;
        //2013/07/16 ICS.居軒 ▲パフォーマンス改善

        //◆基本情報格納変数◆
        internal static Company cCompany = new Company();
        internal static GengoVal cGengo = new GengoVal();
        internal static KaisyaVal cKaisya = new KaisyaVal();
        internal static UsrsecVal cUsrSec = new UsrsecVal();
        internal static UsrtblVal cUsrTbl = new UsrtblVal();
        internal static KankyoVal cKankyo = new KankyoVal();
        internal static VolumVal cVolum = new VolumVal();

        //◆印刷用ディレクトリパス◆
        internal static string sMMDir = "";

        //ダイアログボックスからの返却値
        public static string resCode = "";

        //画面起動モード(0:通常／1:一見先)
        public static int nDispMode = 0;
        public static bool bIchigen = false;
        public static string nIchigenCode = "";

        public static bool bZUpdFlg = false;
        public static int iDispChangeFlg = 0;
        public static int iZCheck = 0;
        public static bool bEnabledState = true;
        public static bool bReadOnlyState = false;

        //変更履歴出力SW
        public static int nRirekiSW = 0;
        public static string sRirekiDate = "";
        public static string sRirekiStartDate = "";
        public static string sRirekiStartTime = "";
        public static string sRirekiStartUser = "";

        //履歴格納用DataTable
        internal static DataTable dtRIREKI;

        //DBコマンド格納用
        internal static System.Data.Common.DbCommand cCmdCommonSel;    //コマンドオブジェクト(Select)(共通)
        internal static System.Data.Common.DbCommand cCmdSelZ;         //コマンドオブジェクト(SELECT)(固定)(財務)
        internal static System.Data.Common.DbCommand cCmdSel;          //コマンドオブジェクト(SELECT)(固定)(SS)
        internal static System.Data.Common.DbCommand cCmdIns;          //コマンドオブジェクト(INSERT)(固定)
        internal static System.Data.Common.DbCommand cCmdInsZ;          //コマンドオブジェクト(INSERT)(固定)
        internal static System.Data.Common.DbCommand cCmdDel;          //コマンドオブジェクト(Delete)(固定)

        //画面項目(支払条件タブ)
        internal static string SHO_ID_tb1;
        internal static string BCOD_tb1;
        internal static string KICD_tb1;
        internal static string KCOD_tb1;
        internal static string SHINO_tb1;
        internal static string SIMEBI_tb1;
        internal static string SHIHARAIMM_tb1;
        internal static string SIHARAIDD_tb1;
        internal static string HARAI_H_tb1;
        internal static string SKIJITUMM_tb1;
        internal static string SKIJITUDD_tb1;
        internal static string KIJITU_H_tb1;
        internal static string SKBNCOD_tb1;
        internal static string V_YAKUJO_tb1;
        internal static string YAKUJOA_L_tb1;
        internal static string YAKUJOA_M_tb1;
        internal static string YAKUJOB_LH_tb1;
        internal static string YAKUJOB_H1_tb1;
        internal static string YAKUJOB_R1_tb1;
        internal static string YAKUJOB_U1_tb1;
        internal static string YAKUJOB_S1_tb1;
        internal static string YAKUJOB_H2_tb1;
        internal static string YAKUJOB_R2_tb1;
        internal static string YAKUJOB_U2_tb1;
        internal static string YAKUJOB_S2_tb1;
        internal static string YAKUJOB_H3_tb1;
        internal static string YAKUJOB_R3_tb1;
        internal static string YAKUJOB_U3_tb1;
        internal static string YAKUJOB_S3_tb1;

        //画面項目(メイン)
        internal static string TRCD;
        internal static string HJCD;
        internal static string TRKBN;
        internal static string RYAKU;
        internal static string TORI_NAM;
        internal static string KNLD;
        internal static string TGASW;
        internal static string ZIP;
        internal static string ADDR1;
        internal static string ADDR2;
        internal static string TEL;
        internal static string FAX;
        internal static string SBUSYO;
        internal static string STANTO;
        internal static string KEICD;
        internal static string STAN;
        internal static string SJBCD;
        internal static string SBCOD;
        internal static string SKICD;
        internal static string NAYOSE;
        internal static string F_SETUIN;
        internal static string F_SHITU;
        internal static string F_ZAN;
        internal static string F_SOUFU;
        internal static string ANNAI;
        internal static string TSOKBN;
        internal static string HORYU;
        internal static string HOVAL;
        internal static string HOKKBN;
        internal static string HODM1;
        internal static string KAIIN;
        internal static string KYKAI;
        internal static string KYVAL;
        internal static string KYCAL;
        internal static string KYZAF;
        internal static string KYZVL;
        internal static string KYZRT;
        internal static string KYZAH;
        internal static string KYZAS;
        internal static string KYROF;
        internal static string KYRVL;
        internal static string KYRRT;
        internal static string KYROH;
        internal static string KYROS;
        internal static string KYGAF;
        internal static string KYGVL;
        internal static string KYGRT;
        internal static string KYGAH;
        internal static string KYGAS;
        internal static string KYKEF;
        internal static string KYKVL;
        internal static string KYKRT;
        internal static string KYKEH;
        internal static string KYKES;
        internal static string GENSEN;                  //
        internal static string GOU;                     //
        internal static string GGKBN;                   //
        internal static string GGKBNM;                  //
        internal static string GSKUBN;                  //
        //internal static string GSSKBN;
        internal static string SZEI;                    //
        internal static string SOSAI;                   //
        internal static string SOKICD;                  //
        internal static string GAIKA;                   //
        internal static string HEI_CD;                  //
        internal static string DM1;                     //
        internal static string DM2;                     //
        internal static string DM3;                     //
        internal static string STYMD;                   //
        internal static string EDYMD;                   //
        internal static string ZSTYMD;                  //
        internal static string ZEDYMD;                  //
        internal static string STFLG;                   //
        internal static string CDM1;                    //
        internal static string LUSR;                    //
        internal static string LMOD;                    //
        internal static string KYZSKBN;                 //
        internal static string KYRSKBN;                 //
        internal static string KYGSKBN;                 //
        internal static string KYKSKBN;                 //
        //**>>ICS-S 2013/05/20
        internal static string CDM2;                    //
        internal static string IDM1;                    //
        internal static string CD03;                    //
        //**<<ICS-E        

        //画面項目(振込先情報タブ)
        internal static string GIN_ID_tb2;              //
        internal static string BANK_CD_tb2;             //
        internal static string SITEN_ID_tb2;            //
        internal static string YOKIN_TYP_tb2;           //
        internal static string KOUZA_tb2;               //
        internal static string MEIGI_tb2;               //
        internal static string MEIGIK_tb2;              //
        internal static string TESUU_tb2;               //
        internal static string SOUKIN_tb2;              //
        internal static string GENDO_tb2;               //

        //画面項目(支払条件タブ)
        internal static string SHO_ID_tb3;              //
        internal static string BCOD_tb3;                //
        internal static string KICD_tb3;                //
        internal static string SHINO_tb3;               //

        internal static string KUBN1_tb3;               //
        internal static string KUBNNM1_tb3;             //
        internal static string OWNID1;                  //
        internal static string BANK1_tb3;               //
        internal static string BANKNM1_tb3;             //
        internal static string SITEN1_tb3;              //
        internal static string SITENNM1_tb3;            //
        internal static string KOZA1_tb3;               //
        internal static string KOZANO1_tb3;             //
        internal static string IRAININ1_tb3;            //
        internal static string FACNAM1_tb3;             //

        internal static string KUBN2_tb3;               //
        internal static string KUBNNM2_tb3;             //
        internal static string OWNID2;                  //
        internal static string BANK2_tb3;               //
        internal static string BANKNM2_tb3;             //
        internal static string SITEN2_tb3;              //
        internal static string SITENNM2_tb3;            //
        internal static string KOZA2_tb3;               //
        internal static string KOZANO2_tb3;             //
        internal static string IRAININ2_tb3;            //
        internal static string FACNAM2_tb3;             //
        
        internal static string KUBN3_tb3;               //
        internal static string KUBNNM3_tb3;             //
        internal static string OWNID3;                  //
        internal static string BANK3_tb3;               //
        internal static string BANKNM3_tb3;             //
        internal static string SITEN3_tb3;              //
        internal static string SITENNM3_tb3;            //
        internal static string KOZA3_tb3;               //
        internal static string KOZANO3_tb3;             //
        internal static string IRAININ3_tb3;            //
        internal static string FACNAM3_tb3;             //
        
        internal static string KUBN4_tb3;               //
        internal static string KUBNNM4_tb3;             //
        internal static string OWNID4;                  //
        internal static string BANK4_tb3;               //
        internal static string BANKNM4_tb3;             //
        internal static string SITEN4_tb3;              //
        internal static string SITENNM4_tb3;            //
        internal static string KOZA4_tb3;               //
        internal static string KOZANO4_tb3;             //
        internal static string IRAININ4_tb3;            //
        internal static string FACNAM4_tb3;             //

        //【SS】で新規に追加された項目
        internal static string TRFURI;                  // 共通：フリガナ
        internal static string SAIKEN;                  // 共通：□得意先
        internal static string SAIKEN_FLG;              // 共通：□入金代表者
        internal static string SAIMU;                   // 共通：□仕入先
        internal static string SAIMU_FLG;               // 共通：□支払代表者
        internal static string GRPID;                   // 共通：取引先グループ
        internal static string GRPIDNM;                 // 共通：取引先グループ名
        internal static string TRMAIL;                  // 基本情報：メールアドレス
        internal static string TRURL;                   // 基本情報：ホームページ
        internal static string BIKO;                    // 基本情報：備考
        internal static string E_TANTOCD;               // 基本情報：営業担当者コード
        internal static string E_TANTONM;               // 基本情報：営業担当者名
        internal static string MYNO_AITE;               // 基本情報：マイナンバー　法人番号
        internal static string SRYOU_F;                 // 基本情報：相殺処理　□相殺領収書を発行する
        internal static string TOKUKANA;                // 回収設定：入金消込設定　照合用フリガナ
        internal static string FUTAN;                   // 回収設定：入金消込設定　手数料負担区分
        internal static string KAISYU;                  // 回収設定：回収予定設定　回収方法
        internal static string YAKUJYO;                 // 回収設定：回収予定設定　約定を指定
        internal static string SHIME;                   // 回収設定：回収予定設定　締日
        internal static string KAISYUHI;                // 回収設定：回収予定設定　回収予定（MDD）
        internal static string KAISYUSIGHT;             // 回収設定：回収予定設定　回収期日（MDD）
        internal static string Y_KINGAKU;               // 回収設定：回収予定設定　約定金額
        internal static string HOLIDAY;                 // 回収設定：回収予定設定　休業日設定
        internal static string MIMAN;                   // 回収設定：回収予定設定　約定金額未満
        internal static string IJOU_1;                  // 回収設定：回収予定設定　約定金額以上①
        internal static string BUNKATSU_1;              // 回収設定：回収予定設定　分割①
        internal static string HASU_1;                  // 回収設定：回収予定設定　端数単位①
        internal static string SIGHT_1;                 // 回収設定：回収予定設定　回収サイト①
        internal static string IJOU_2;                  // 回収設定：回収予定設定　約定金額以上②
        internal static string BUNKATSU_2;              // 回収設定：回収予定設定　分割②
        internal static string HASU_2;                  // 回収設定：回収予定設定　端数単位②
        internal static string SIGHT_2;                 // 回収設定：回収予定設定　回収サイト②
        internal static string IJOU_3;                  // 回収設定：回収予定設定　約定金額以上③
        internal static string BUNKATSU_3;              // 回収設定：回収予定設定　分割③
        internal static string HASU_3;                  // 回収設定：回収予定設定　端数単位③
        internal static string SIGHT_3;                 // 回収設定：回収予定設定　回収サイト③
        internal static string SEN_GINKOCD;             // 回収設定：専用入金口座　銀行コード
        internal static string SEN_GINKONM;             // 回収設定：専用入金口座　銀行名
        internal static string SEN_SITENCD;             // 回収設定：専用入金口座　支店コード
        internal static string KASO_SITENCD;            // 回収設定：専用入金口座　仮想支店コード
        internal static string KASO_SITENNM;            // 回収設定：専用入金口座　仮想支店名
        internal static string YOKINSYU;                // 回収設定：専用入金口座　預金種別
        internal static string SEN_KOZANO;              // 回収設定：専用入金口座　口座番号
        internal static string JIDOU_GAKUSYU;           // 回収設定：各設定　□カナ自動学習
        internal static string NYUKIN_YOTEI;            // 回収設定：各設定　□入金予定利用
        internal static string TESURYO_GAKUSYU;         // 回収設定：各設定　□手数料自動学習する
        internal static string TESURYO_GOSA;            // 回収設定：各設定　□手数料誤差利用する
        internal static string RYOSYUSYO;               // 回収設定：各設定　□領収書発行する
        internal static string SHIN_KAISYACD;           // 回収設定：各設定　信用調査用企業コード
        internal static string YOSIN;                   // 回収設定：各設定　与信限度額
        internal static string YOSHINRANK;              // 回収設定：各設定　与信ランク
        internal static string TSUKA;                   // 回収設定：外貨関連　取引通貨
        internal static string GAIKA_KEY_F;             // 回収設定：外貨関連　照合ｷｰ（前）
        internal static string GAIKA_KEY_B;             // 回収設定：外貨関連　照合ｷｰ（後）
        internal static string HIFURIKOZA_1;            // 回収設定：被振込口座設定　被振込口座１（自社銀行キー）
        internal static string HIBKCD_1;                // 回収設定：被振込口座設定　被振込口座１（銀行コード）
        internal static string HIBKNM_1;                // 回収設定：被振込口座設定　被振込口座１（銀行名）
        internal static string HIBRCD_1;                // 回収設定：被振込口座設定　被振込口座１（支店コード）
        internal static string HIBRNM_1;                // 回収設定：被振込口座設定　被振込口座１（支店名）
        internal static string HIYOKN_1;                // 回収設定：被振込口座設定　被振込口座１（預金種別）
        internal static string HIKOZA_1;                // 回収設定：被振込口座設定　被振込口座１（口座番号）
        internal static string HIFURIKOZA_2;            // 回収設定：被振込口座設定　被振込口座２
        internal static string HIBKCD_2;                // 回収設定：被振込口座設定　被振込口座２（銀行コード）
        internal static string HIBKNM_2;                // 回収設定：被振込口座設定　被振込口座２（銀行名）
        internal static string HIBRCD_2;                // 回収設定：被振込口座設定　被振込口座２（支店コード）
        internal static string HIBRNM_2;                // 回収設定：被振込口座設定　被振込口座２（支店名）
        internal static string HIYOKN_2;                // 回収設定：被振込口座設定　被振込口座２（預金種別）
        internal static string HIKOZA_2;                // 回収設定：被振込口座設定　被振込口座２（口座番号）
        internal static string HIFURIKOZA_3;            // 回収設定：被振込口座設定　被振込口座３
        internal static string HIBKCD_3;                // 回収設定：被振込口座設定　被振込口座３（銀行コード）
        internal static string HIBKNM_3;                // 回収設定：被振込口座設定　被振込口座３（銀行名）
        internal static string HIBRCD_3;                // 回収設定：被振込口座設定　被振込口座３（支店コード）
        internal static string HIBRNM_3;                // 回収設定：被振込口座設定　被振込口座３（支店名）
        internal static string HIYOKN_3;                // 回収設定：被振込口座設定　被振込口座３（預金種別）
        internal static string HIKOZA_3;                // 回収設定：被振込口座設定　被振込口座３（口座番号）
        internal static string GAI_F;                   // 支払条件：◎取引区分
        internal static string OWNID1_tb3;              // 支払条件：口座ID1
        internal static string OWNID2_tb3;              // 支払条件：口座ID2
        internal static string OWNID3_tb3;              // 支払条件：口座ID3
        internal static string OWNID4_tb3;              // 支払条件：口座ID4
        internal static string FDEF;                    // 振込先情報：□初期値
        internal static string DDEF;                    // 振込先情報：□でんさい代表口座
        internal static string FTESUID;                 // 振込先情報：銀行振込　手数料ID
        internal static string DTESUSW;                 // 振込先情報：全銀電子債権ネットワーク　□手数料設定を使用する
        internal static string DTESU;                   // 振込先情報：全銀電子債権ネットワーク　手数料負担
        internal static string TEGVAL;                  // その他情報：手形関連　送料
        internal static string GSSKBN;                  // その他情報：源泉税関連　計算基準
        internal static string HR_KIJYUN;               // その他情報：控除関連　計算摘要基準額
        internal static string HRORYUGAKU;              // その他情報：控除関連　定額
        internal static string HORYU_F;                 // その他情報：控除関連　計算区分フラグ
        internal static string HRKBN;                   // その他情報：控除関連　作成区分
        internal static string GAI_SF;                  // 外貨設定：◎送金種類
        internal static string GAI_SH;                  // 外貨設定：◎送金支払方法
        internal static string GAI_KZID;                // 外貨設定：出金口座
        internal static string GAI_TF;                  // 外貨設定：手数料負担
        internal static string ENG_NAME;                // 外貨設定：英語表記　受取人名
        internal static string ENG_ADDR;                // 外貨設定：英語表記　住所
        internal static string ENG_KZNO;                // 外貨設定：外国向け送金設定　口座番号
        internal static string ENG_SWIF;                // 外貨設定：外国向け送金設定　SWIFTコード
        internal static string ENG_BNKNAM;              // 外貨設定：外国向け送金設定　被仕向銀行名
        internal static string ENG_BRNNAM;              // 外貨設定：外国向け送金設定　被仕向支店名
        internal static string ENG_BNKADDR;             // 外貨設定：外国向け送金設定　被仕向銀行住所






        //印刷設定
        internal static int Prn_SortKEY;
        internal static int Prn_PagingTRCD;
        internal static int Prn_PKind;
        internal static int Prn_PType;
        internal static string Prn_TRCD_Sta;
        internal static string Prn_HJCD_Sta;
        internal static string Prn_TRCD_End;
        internal static string Prn_HJCD_End;
        internal static string Prn_TRCD_Once;
        internal static bool Prn_ZSTYMD_Null;
        internal static bool Prn_ZEDYMD_Null;
        internal static bool Prn_STYMD_Null;
        internal static bool Prn_EDYMD_Null;
        internal static int Prn_ZSTYMD_Sta;
        internal static int Prn_ZSTYMD_End;
        internal static int Prn_ZEDYMD_Sta;
        internal static int Prn_ZEDYMD_End;
        internal static int Prn_STYMD_Sta;
        internal static int Prn_STYMD_End;
        internal static int Prn_EDYMD_Sta;
        internal static int Prn_EDYMD_End;
        internal static int Prn_Address;
        internal static int Prn_Kaisyu;
        internal static int Prn_Frigin;
        internal static int Prn_Shiharai;
        internal static int Prn_Others;
        internal static int Prn_Gaika;
        internal static int Prn_Master;
        internal static int Prn_Kind;
        internal static int SearchMode;
        internal static string[,] sTRCDArray = null;  //取引先CD,補助CD格納用
        internal static int PrnTarget;

        //画面項目制御用
        internal static int nKMAN;                                  // 起票者　使用するフラグ　財務から
        internal static int nGengo;                                 // 会社マスター　元号フラグ
        internal static int nKANRI_F;                               // 未使用？

        internal static int nSAIKEN_F;                              // 債権使用する
        internal static int nSAIMU_F;                               // 債務使用する
        internal static int nKIJITU_F;                              // 期日管理使用する
//-- <2016/02/14 追加>
        internal static int nSOSAI_F;                               // 相殺使用する
        internal static int nGAIKA_F;                               // 外貨使用する
//-- <2016/02/14>

        internal static int nSAIKEN_F_USec;
        internal static int nSAIMU_F_USec;
        internal static int nKIJITU_F_USec;

        internal static int nGroup;                                 // 取引先グループ使用する(管理テーブル)

        internal static int nF_ICHI;
        internal static int nTRCD_Type;
        internal static int nTRCD_Len;
        internal static int nTRCD_ZE;
        internal static int nTRCD_HJ;
        internal static int nKCOD_Type;
        internal static int nKCOD_Len;
        internal static int nKCOD_ZE;
        internal static int nBCD_ZMAK;
        internal static int nBCOD_F;
        internal static int nBCOD_Type;
        internal static int nBCOD_Len;
        internal static int nBCOD_ZE;
        internal static int nEDCOD_F;
        internal static int nEDCOD_Type;
        internal static int nEDCOD_Len;
        internal static int nEDCOD_ZE;
        internal static int nHSSW;
        internal static int nETAN_Type;                             // 
        internal static int nETAN_Len;
        internal static int nF_SENYOU;
        internal static int nKanri_IDATA = 0;//<---V01.12.01 ATT ADD ◀(8084)

        internal static DateTime dNow;

        internal static string TRCD_R = "";
        internal static string RYAKU_R = "";
        internal static string TORI_NAM_R = "";
        internal static string KNLD_R = "";
        internal static string FUSR_R = "";
        internal static string FMOD_R = "";
        internal static string FTIM_R = "";
        internal static string LUSR_R = "";
        internal static string LMOD_R = "";
        internal static string LTIM_R = "";

        internal static bool bRKFLG = false;
        internal static int[] nTRFLG = new int[2];
        internal static int[] nGCFLG = new int[2];
        internal static int[] nSYMD = new int[2];
        internal static int[] nEYMD = new int[2];

        internal static DataTable dtTSHOH;

        internal static bool bZaimBoot = false;

        #region
        internal static bool bUpdated;
        internal static int nExpPrn;
        internal static bool bSaimuTori_Dsp;
        internal static bool bInKikan_Dsp;
        internal static bool bTRCD_HJ;
        internal static bool bUseKikan_Dsp;
        internal static bool bChkSaimuUse;
        internal static int nFont;
        internal static string strFont;
        internal static int nVolKJUN;
        #endregion

//-- <2016/02/17 起動チェック用等>
        internal static bool bSub801;       // 債権使用（会社別サブシステム）
        internal static bool bSub802;       // 債務使用（会社別サブシステム）
        internal static bool bSub803;       // 相殺使用（会社別サブシステム）
        internal static bool bSub804;       // 期日使用（会社別サブシステム）
        internal static int nF_SAIKEN;      // 正規の債権使用フラグ
        internal static int nF_SAIMU;       // 正規の債務使用フラグ
        internal static int nF_SOSAI;       // 正規の相殺使用フラグ
        internal static int nF_KJTKAN;      // 正規の期日使用フラグ
//-- <2016/02/17>

        // ---> V02.23.02 KSM ADD ▼(mutexでの多重起動チェックの実装対応)
        /// <summary>
        /// 多重起動チェック用ミューテックス
        /// </summary>
        internal static System.Threading.Mutex cMutex;
        // <--- V02.23.02 KSM ADD ▲(mutexでの多重起動チェックの実装対応)

        /// <summary>
        /// メインフォームのタイトルバーの文字列取得（業務名称、バージョン） 
        /// </summary>
        /// <returns></returns>
        internal static string GetMainFormCaption()
        {
            if (nDebugFlg == 0)
            {
                return sPrgName + " Ver" + sPrgVer;
            }
            else
            {
                return sPrgName;
            }
        }

        /// <summary>
        /// コマンドライン引数から会社コード、ユーザIDを取得して、メンバ変数に設定 
        /// </summary>
        /// <param name="sArgArrayParam">コマンドライン引数・文字列配列</param>
        /// <returns></returns>
        internal static bool SetArgArray(string[] sArgArrayParam)
        {
            sArgArray = sArgArrayParam;

            if (!ComInfo.IcsChkCommandLine(sArgArray))
            {
                return false;
            }
            sCcod = sArgArray[1];                   // 会社コードの取得
            nUcod = int.Parse(sArgArray[2]);        // ユーザIDの取得
            nDebugFlg = int.Parse(sArgArray[3]);
            if (sArgArray.Length == 6)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "起動パラメーターが不正です。\n取引先詳細情報登録を中止します。",
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    "起動パラメーターが不正です。\n取引先詳細情報登録を中止します。\n\n　Ver" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                Environment.Exit(0);
            }
            if (sArgArray.Length >= 9)
            {
                if (Global.nTRCD_Type == 0)
                {
                    Global.sTRCD_R = sArgArrayParam[6].PadLeft(Global.nTRCD_Len, '0');
                }
                else
                {
                    Global.sTRCD_R = sArgArrayParam[6].TrimEnd(' ');
                }
                Global.sTRCD_R = sArgArrayParam[6];
                Global.sHJCD_R = sArgArrayParam[7].PadLeft(6, '0');
                Global.sTRNAM_R = sArgArrayParam[8];
                Global.bIchigen = !String.IsNullOrEmpty(Global.sTRNAM_R);
                if (sArgArray.Length >= 10)
                {
                    Global.bZMode = (sArgArrayParam[9] == "2" ? true : false);
                    Global.nShTgSW = (sArgArray.Length == 9 ? 0 : int.Parse(sArgArrayParam[9]));
                }
            }
            return true;
        }

        /// <summary>
        /// メインフォームのタイトルバーの文字列取得（業務名称、バージョン、会社コード、会社名称） 
        /// </summary>
        /// <param name="cKaisya"></param>
        /// <returns></returns>
        internal static string GetMainFormCaption(KaisyaVal cKaisya)
        {
            if (nDebugFlg == 0)
            {
                // メインフォームのタイトルバーの文字列（業務名称  バージョン - [会社コード]会社名称）を返す。
                // Ver.01.02.05 Toda -->
                //return GetMainFormCaption() + " - [" + cKaisya.sCCOD + "]" + cKaisya.sCNAM;
                return GetMainFormCaption() + " - [" + cKaisya.sCCOD + "]" + IcsSSSInfo.SSSInfo.sCNAM;
                // Ver.01.02.05 <--
            }
            else
            {
                return GetMainFormCaption();
            }
        }

        internal static Hashtable htChange;

        internal static void MakeHash()
        {
            htChange.Add('ｧ', 'ｱ');
            htChange.Add('ｨ', 'ｲ');
            htChange.Add('ｩ', 'ｳ');
            htChange.Add('ｪ', 'ｴ');
            htChange.Add('ｫ', 'ｵ');
            htChange.Add('ｯ', 'ﾂ');
            htChange.Add('ｬ', 'ﾔ');
            htChange.Add('ｭ', 'ﾕ');
            htChange.Add('ｮ', 'ﾖ');
            htChange.Add('ｰ', '-');
        }

        internal static string EscapeSqlLike(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return String.Empty;
            }

            return Regex.Replace(s, @"([_%\[])", "[$1]");
        }
        internal static string EscapeQuote(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s.Replace("'", "''");
        }
        #region 変換用

        // 変更用
        public static string ChangeCharacter(string cText)
        {
            string sValue = Strings.StrConv(cText, VbStrConv.Katakana | VbStrConv.Narrow, 0);
            return ChangeSmallCharacter(sValue);
        }

        /// <summary>
        /// 文字が半角文字かチェックする
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        internal static bool CheckHankaku(string sValue)
        {
            if (Regex.IsMatch(sValue, "^[a-zA-Z0-9!-/:-@¥[-`{-~]+$"))
            {
                // 半角英数記号
                return true;
            }
            if (Regex.IsMatch(sValue, @"[\uFF61-\uFF9F]"))
            {
                // 半角カタカナ

                return true;
            }

            return false;
        }

        internal static string ChangeSmallCharacter(string sValue)
        {
            // 拗音を変換
            char[] c = sValue.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (Global.htChange.Contains(c[i]))
                {
                    c[i] = (char)Global.htChange[c[i]];
                }
            }

            return new string(c);
        }
        #endregion

        /// <summary>
        /// 法人格略称
        /// </summary>
        private static readonly List<string> abbreviations = new List<string>()
        {
            "ｼﾖｸﾊﾝｷﾖｳ",
            "ｺｳｷﾖｳﾚﾝ",
            "ｺｸｷﾖｳﾚﾝ",
            "ﾉｳｷﾖｳﾚﾝ",
            "ｹｲｻﾞｲﾚﾝ",
            "ｷﾖｳｻｲﾚﾝ",
            "ｶｲｼﾞﾖｳ",
            "ｼﾞﾕｳｸﾐ",
            "ｷﾞﾖｷﾖｳ",
            "ｾｲｷﾖｳ",
            "ｷﾞﾖﾚﾝ",
            "ｼﾔｷﾖｳ",
            "ｼﾖｸｱﾝ",
            "ｷﾖｳｻｲ",
            "ｷﾖｳｸﾐ",
            "ｺｸﾎﾚﾝ",
            "ﾛｳｸﾐ",
            "ｺｳﾈﾝ",
            "ｾｲﾒｲ",
            "ﾄｸﾖｳ",
            "ﾕｳｸﾐ",
            "ｹﾝﾎﾟ",
            "ｺｸﾎ",
            "ｼﾔﾎ",
            "ｶｻｲ",
            "ﾄｸﾋ",
            "ｶﾝﾘ",
            "ﾛｳﾑ",
            "ｶﾌﾞ",
            "ｻﾞｲ",
            "ｼﾕﾂ",
            "ｼﾕｳ",
            "ﾁﾕｳ",
            "ｶﾞｸ",
            "ﾎｺﾞ",
            "ﾕｳ",
            "ｼﾔ",
            "ﾌｸ",
            "ｴｲ",
            "ﾚﾝ",
            "ﾄﾞｸ",
            "ﾍﾞﾝ",
            "ｷﾞﾖ",
            "ｼﾎｳ",
            "ｾﾞｲ",
            "ﾀﾞｲ",
            "ﾉｳ",
            "ﾄﾞ",
            "ﾕ",
            "ﾒ",
            "ｿ",
            "ｼ",
            "ｶ",
            "ｲ",
        };

        internal static string RemoveHojinKaku(string sValue)
        {
            var patterns = new string[] { "({0})", "{0})", "({0}" };
            foreach (var symbols in abbreviations)
            {
                foreach (var pattern in patterns)
                {
                    var target = string.Format(pattern, symbols);
                    if (!sValue.Contains(target)) continue;
                    sValue = sValue.Replace(target, "");
                }
            }

            return sValue;
        }

        internal static int ObjectToInt(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            else
            {
                int oInt;
                return int.TryParse(obj.ToString(), out oInt) ? oInt : 0;
            }
        }


    }
}
