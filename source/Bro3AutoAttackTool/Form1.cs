using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace Bro3AutoAttackTool
{

    public partial class Form1 : Form
    {
        const bool TOOL_MODE = true;
        const int ZG_ID = 999;

        bool timerflg = false;
        int timercount = 0;

        int wood=0;
        int stone = 0;
        int iron = 0;
        int rice = 0;
        int res = 0;

        int base_wood = 0;
        int base_stone = 0;
        int base_iron = 0;
        int base_rice = 0;
        
        public struct autoskillparams
        {
            public int status ;
            public int tabnum ;
            public int pagenum;
            public int maxpagenum;
            public string skillname;
            public string settype;
            public int actiontype;
        }
        autoskillparams asp;
        Dictionary<string, int> curPagenum = new Dictionary<string, int>();

        string bname = string.Empty;
        string bskill = string.Empty;
        string mtype = string.Empty;

        bool rflg = false;

        int jsIdx = 0;

        SelectZengunBusyo szbdlg = new SelectZengunBusyo();

        const string FEATURE_BROWSER_EMULATION = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
        Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(FEATURE_BROWSER_EMULATION);
        string process_name = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
 
        public Form1()
        {
            InitializeComponent();

            this.Text = string.Format("{0} Ver.{1}", this.Text, Version.ver);

            regkey.SetValue(process_name, 11001, Microsoft.Win32.RegistryValueKind.DWord);

            this.setColor();
            if (TOOL_MODE == false) { worldid.ReadOnly = true; }

#if DEBUG
            testbtn.Visible = true;
#endif

        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //読み込み完了確認
            if (((WebBrowser)sender).ReadyState == WebBrowserReadyState.Complete)
            {
                //カウントダウンタイマーリセット
                timercount = int.Parse(spannum.Value.ToString());

                //画面の中央を表示
                wb.Document.Window.ScrollTo(new System.Drawing.Point(0, wb.Document.Body.ScrollRectangle.Size.Height / 2));
                WebBrowser w = (WebBrowser)sender;

                if (w.Url.ToString().Substring(0, 7 + worldid.Text.Length) == string.Format("http://{0}", worldid.Text))
                {
                    this.SetCurrentResource();

                    string url = w.Url.ToString().Replace(string.Format("http://{0}.3gokushi.jp", worldid.Text), "");
                    debug_log(url);
                    switch (url)
                    {
                        //出兵画面
                        case "/facility/castle_send_troop.php":

                            //village check
                            if (this.CheckVillage()) { break; }

                            //busyo list
                            debug_log("load comp:/facility/castle_send_troop.php" + System.Environment.NewLine + w.Url.ToString());
                            List<Busyo> bList = this.GetBusyoList(w);

        
                            //回復チェック
                            if (chkjinkun.Checked)
                            {
                                bool needjinkun = true;
                                bool kdflg = false;
                                foreach (Busyo bu in bList)
                                {
                                    //鹵獲武将以外は無視
                                    if (bu.attack < decimal.ToInt32(attackBorder.Value)) { continue; }
                                    if (bu.speed < decimal.ToInt32(speedBorder.Value)) { continue; }

                                    if (bu.Id.Equals(string.Empty)) { needjinkun = false; }

                                    //攻奪は帰還待たず回復
                                    if (koudatuHP.Checked)
                                    {
                                        foreach (string kd in this.getKoudatuList())
                                        {
                                            if (bu.skill.Contains(kd.Trim())) { kdflg = true; }
                                        }
                                    }
                                }
                                if(needjinkun || kdflg){
                                    foreach (Busyo bu in bList)
                                    {
                                        //鹵獲武将以外は無視
                                        if (bu.attack < decimal.ToInt32(attackBorder.Value)) { continue; }
                                        if (bu.speed < decimal.ToInt32(speedBorder.Value)) { continue; }                                      

                                        if (bu.hp < decimal.ToInt32(jinkunhp.Value))
                                        {
                                            //全軍をおろす
                                            if (zgFlg.Checked)
                                            {
                                                foreach (Busyo bu2 in bList)
                                                {
                                                    if (bu2.Name.Equals(zgName.Text.Trim()))
                                                    {
                                                        wb.Navigate(string.Format("http://{0}.3gokushi.jp/card/deck.php#zgof", worldid.Text));
                                                        return;
                                                    }
                                                }
                                            }

                                            List<string> jsList = this.getJinkunSkillList();
                                            if (jsIdx >= jsList.Count) { jsIdx = 0; }
                                            if (asp.status == 0)
                                            {
                                                asp.status = 1;
                                                asp.tabnum = decimal.ToInt32(jinkuntab.Value);                                                
                                                asp.maxpagenum = decimal.ToInt32(jinkunpage.Value);
                                                asp.skillname = jsList[jsIdx];  //"仁君";
                                                asp.pagenum = this.GetCurrentPagdenum();
                                                asp.settype = "domestic_set";
                                                asp.actiontype = 2;

                                                wb.Navigate(string.Format("http://{1}.3gokushi.jp/card/deck.php?l={0}&p={2}"
                                                    , asp.tabnum.ToString(), worldid.Text, asp.pagenum.ToString()));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }

                            //傾国チェック
                            if (chkKeikoku.Checked)
                            {
                                bool needKeikoku = true;
                                if (0 == bList.Count) { needKeikoku = false; }
                                foreach (Busyo bu in bList)
                                {
                                    //鹵獲武将以外は無視
                                    if (bu.attack < decimal.ToInt32(attackBorder.Value)) { continue; }
                                    if (bu.speed < decimal.ToInt32(speedBorder.Value)) { continue; }
                                    
                                    //攻奪は傾国無視
                                    bool kdflg = false;
                                    foreach (string kd in this.getKoudatuList())
                                    {
                                        if (bu.skill.Contains(kd.Trim())) { kdflg = true; }
                                    }
                                    if (kdflg) { continue; }

                                    //帰還してないor討伐が足りてる場合は傾国を使わない
                                    if (bu.Id.Equals(string.Empty) || bu.tobatu >= decimal.ToInt32(toubatu.Value)) { needKeikoku = false; }
                                }
                                if (needKeikoku)
                                {
                                    //全軍をおろす
                                    if (zgFlg.Checked)
                                    {
                                        foreach (Busyo bu2 in bList)
                                        {
                                            if (bu2.Name.Equals(zgName.Text.Trim()))
                                            {
                                                wb.Navigate(string.Format("http://{0}.3gokushi.jp/card/deck.php#zgof", worldid.Text));
                                                return;
                                            }
                                        }
                                    }

                                    if (asp.status == 0)
                                    {
                                        asp.status = 1;
                                        asp.tabnum = decimal.ToInt32(keikokutab.Value);
                                        asp.maxpagenum = decimal.ToInt32(keikokuPageNum.Value);
                                        asp.skillname = "傾国";
                                        asp.pagenum = this.GetCurrentPagdenum();
                                        asp.settype = "domestic_set";
                                        asp.actiontype = 2;

                                        wb.Navigate(string.Format("http://{1}.3gokushi.jp/card/deck.php?l={0}&p={2}"
                                            , asp.tabnum.ToString(), worldid.Text, asp.pagenum.ToString()));
                                        return;
                                    }
                                }
                            }


                            //syuppei
                            foreach (Busyo bu in bList)
                            {
                                if (!bu.Id.Equals(string.Empty))
                                {
                                    bname = bu.Name;
                                    bskill = string.Empty;

                                    //busyosentaku
                                    HtmlElement el = w.Document.GetElementById(bu.Id);
                                    el.SetAttribute("Checked", "True");

                                    //skill sentaku
                                    if (!bu.skillId.Equals(string.Empty))
                                    {
                                        w.Document.GetElementById(bu.skillId).SetAttribute("Checked", "True");
                                        bskill = bu.skillName;
                                    }

                                    //zahyou settei
                                    Point p = GetSyuppeiPoint(bu.Name);
                                    foreach (HtmlElement inp in w.Document.GetElementsByTagName("input"))
                                    {
                                        if (inp.GetAttribute("Name").Equals("village_x_value"))
                                        {
                                            inp.InnerText = p.X.ToString();
                                        }
                                        if (inp.GetAttribute("Name").Equals("village_y_value"))
                                        {
                                            inp.InnerText = p.Y.ToString();
                                        }
                                    }

                                    //toubatutyekku
                                    if (toubatuChk.Checked && bu.tobatu < toubatu.Value) { continue; }

                                    //attack check
                                    if (bu.attack < attackBorder.Value) { continue; }

                                    //speed check
                                    if (bu.speed < speedBorder.Value) { continue; }

                                    //HP　check
                                    if (chkjinkun.Checked && bu.hp < jinkunhp.Value) { continue; }


                                    if (rflg)
                                    {
                                        w.Document.GetElementById("btn_preview").InvokeMember("click");
                                    }
                                    return;
                                }
                            }

                            //全軍を載せる
                            if (zgFlg.Checked)
                            {
                                bool zg = true;
                                foreach (Busyo bu2 in bList)
                                {
                                    if (bu2.Name.Equals(zgName.Text.Trim())) { zg = false; }
                                }
                                if (zg)
                                {
                                    asp.status = ZG_ID;
                                    asp.tabnum = decimal.ToInt32(zgTab .Value);
                                    asp.maxpagenum = decimal.ToInt32(zgPage.Value);
                                    asp.skillname = "全軍攻";
                                    asp.pagenum = this.GetCurrentPagdenum();
                                    asp.settype = "set";
                                    asp.actiontype = 0;
                                    wb.Navigate(string.Format("http://{1}.3gokushi.jp/card/deck.php?l={0}&p={2}"
                                        , asp.tabnum.ToString(), worldid.Text, asp.pagenum.ToString()));
                                    return;
                                }
                            }

                            break;
                        //出兵画面
                        case "/facility/castle_send_troop.php#ptop":
                            if (rflg)
                            {
                                foreach (HtmlElement el in w.Document.GetElementsByTagName("td"))
                                {
                                    if (el.GetAttribute("className").Equals("bbno brno location"))
                                    {
                                        //check akiti
                                        if (chkakiti.Checked)
                                        {
                                            if (el.GetElementsByTagName("strong")[0].InnerHtml.IndexOf("【空き地】") < 0)
                                            {
                                                log(string.Format("空き地チェック違反で出兵停止 - [1] {0}", el.GetElementsByTagName("strong")[0].InnerHtml, mtype));
                                                rflg = false;
                                                return;
                                            }
                                        }

                                        log(string.Format("{0} [{3}] - {1} {2}"
                                            , el.GetElementsByTagName("strong")[0].InnerHtml
                                            , bname, bskill, mtype));
                                    }
                                }
                                HtmlElement he = w.Document.GetElementById("btn_send");
                                if (he != null) { he.InvokeMember("click"); }
                                rflg = false;
                            }
                            break;

                        case "/card/deck.php#zgof":
                            //全軍武将を下ろす
                            HtmlElementCollection fs = wb.Document.GetElementsByTagName("form");
                            foreach (HtmlElement form in wb.Document.GetElementsByTagName("form"))
                            {
                                if (!form.GetAttribute("className").Equals("clearfix")) { continue; }

                                foreach (HtmlElement div in wb.Document.GetElementsByTagName("div"))
                                {
                                    if (!div.GetAttribute("className").Equals("cardColmn")) { continue; }

                                    foreach (HtmlElement span in div.GetElementsByTagName("span"))
                                    {
                                        if (span.GetAttribute("className").Equals("cardno")) 
                                        {
                                            if (span.InnerHtml.Equals(zgNo.Value.ToString()))
                                            {
                                                //デッキから下ろす全軍武将発見
                                                try
                                                {
                                                    foreach (HtmlElement img in div.GetElementsByTagName("img"))
                                                    {
                                                        if (!img.GetAttribute("className").Equals("set_release btn_deck_set")) { continue; }


                                                        //ヘッダー情報
                                                        string str_header = "Content-Type: application/x-www-form-urlencoded;charset=UTF-8";
                                                        System.Text.RegularExpressions.Regex r =
                                                            new System.Text.RegularExpressions.Regex(@"operationExecution\('.*, 'unset'\)");
                                                        System.Text.RegularExpressions.Match m = r.Match(div.InnerHtml);

                                                        if (m.Success)
                                                        {
                                                            string idnum = m.Value.Split(',')[1].Trim();

                                                            string ssid = wb.Document.GetElementById("ssid").GetAttribute("value");
                                                            string data = string.Format("mode=unset&target_card={0}&wild_card_flg=&inc_point=&btn_change_flg=&l=&ssid={1}", idnum, ssid);
                                                            byte[] byte_post = Encoding.ASCII.GetBytes(data);

                                                            wb.Navigate(string.Format("http://{0}.3gokushi.jp/card/deck.php", worldid.Text), null, byte_post, str_header);

                                                            log(string.Format("全軍攻カード：No.{0} {1} をデッキから降ろしました", zgNo.Value, zgName.Text));
                                                            
                                                            return;

                                                        }
                                                    }
                                                }
                                                catch (Exception exp) { }
                                                return;
                                            }
                                        }
                                    }                                                                                                        
                                }
                            }
                            break;
                    }

                    if (url.IndexOf("/card/deck.php?l=") == 0)
                    {
                        //デッキタブ表示

                            HtmlElement bli = w.Document.GetElementById("cardFileList");
                            if (bli == null) { return; }

                            foreach (HtmlElement el in bli.GetElementsByTagName("div"))
                            {
                                if (!el.GetAttribute("className").Equals("statusDetail clearfix")) { continue; }

                                bool skillcheck = false;
                                //全軍攻撃カード
                                if (zgFlg.Checked)
                                {
                                    if (el.InnerHtml.ToUpper().IndexOf(string.Format("<TD>{0}</TD>", zgNo.Value))>=0
                                        && el.InnerHtml.ToUpper().IndexOf(string.Format("<TD>{0}</TD>", zgName.Text))>=0
                                        )
                                    {
                                        skillcheck = true;
                                    }
                                }

                                //if (skillcheck)
                                {
                                    //updeck check
                                    foreach (HtmlElement div in el.GetElementsByTagName("div"))
                                    {
                                        if (div.GetAttribute("className").Equals("set"))
                                        {
                                            foreach (HtmlElement a in div.GetElementsByTagName("a"))
                                            {
                                                string href = a.GetAttribute("href");
                                                if (href.Length > 0)
                                                {
                                                    //ヘッダー情報
                                                    string str_header = "Content-Type: application/x-www-form-urlencoded;charset=UTF-8";
                                                    
                                                    //uriからスキル情報取得
                                                    Uri uri = new Uri(href);
                                                    System.Collections.Specialized.NameValueCollection querys = HttpUtility.ParseQueryString(uri.ToString());
                                                    List<Dictionary<string, string>> skills = new List<Dictionary<string, string>>();
                                                    skills.Add(new Dictionary<string, string>());
                                                    int idx = 0;
                                                    foreach (string q in uri.ToString().Split('&'))
                                                    {
                                                        string[] v = q.Split('=');
                                                        if (v[0].Equals("index"))
                                                        {
                                                            idx = int.Parse(v[1]);
                                                            if (idx > 0) { skills.Add(new Dictionary<string, string>()); }
                                                        }
                                                        else
                                                        {
                                                            if (v.Length >= 2) { skills[idx].Add(v[0], v[1]); }
                                                        }
                                                    }

                                                    foreach(Dictionary<string, string> skill in skills)
                                                    {
                                                        string sid = string.Empty;
                                                        string cid = string.Empty;
                                                        if (!skillcheck && skill.Count > 0)
                                                        {
                                                            if (!skill["recovery_time"].Equals("0")) { continue; }
                                                            if (skill["skill_name"].IndexOf(asp.skillname) < 0) { continue; }

                                                            sid = skill["skill_id"];
                                                            cid = skill["card_id"];
                                                        }
                                                        else
                                                        {
                                                            foreach (HtmlElement img in div.GetElementsByTagName("img"))
                                                            {
                                                                if (img.GetAttribute("className").Equals("aboutdeck set_release"))
                                                                {
                                                                    System.Text.RegularExpressions.Regex r =
                                                                        new System.Text.RegularExpressions.Regex(@"tbShowDeckSetPopup\(event, 'Cardset_master',.*\)");
                                                                    System.Text.RegularExpressions.Match m = r.Match(div.InnerHtml);

                                                                    if (m.Success)
                                                                    {
                                                                        cid = m.Value.Split(',')[2].Replace(")", "").Trim();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        
                                                        
                                                        string ssid = wb.Document.GetElementById("ssid").GetAttribute("value");
                                                        bool skillvillageflg = true;
                                                        if (asp.status == ZG_ID) { skillvillageflg = false; }
                                                        string data = string.Format("mode={7}&target_card={0}&l={3}&p={4}&ssid={1}&selected_village[{0}]={2}&action_type={5}&choose_attr1_skill={6}"
                                                            , cid, ssid, this.GetSkillVillageId(skillvillageflg), asp.tabnum, asp.pagenum, asp.actiontype, sid, asp.settype);
                                                        byte[] byte_post = Encoding.ASCII.GetBytes(data);


                                                        if (asp.status == 1)
                                                        {
                                                            asp.status = 2;
                                                            wb.Navigate(string.Format("http://{0}.3gokushi.jp/card/deck.php", worldid.Text), null, byte_post, str_header);
                                                            string village = string.Empty;
                                                            if (skillvillagechk.Checked) { village = string.Format("@{0}", skillvillages.Text); }
                                                            log(string.Format("スキル実行：{0} {1}", skill["skill_name"], village).Trim());
                                                        }
                                                        else if (asp.status == ZG_ID && skillcheck)
                                                        {
                                                            wb.Navigate(string.Format("http://{0}.3gokushi.jp/card/deck.php", worldid.Text), null, byte_post, str_header);
                                                            log(string.Format("全軍攻カード：No.{0} {1} をデッキに載せました", zgNo.Value, zgName.Text));
                                                        }
                                                        return;
                                                    }

                                                }
                                            }
                                        }
                                    }
  
                                }
                            }

                        //スキルがが見つからなかった
                        asp.pagenum++;
                        if (asp.pagenum <= asp.maxpagenum)
                        {
                            curPagenum[asp.skillname] = asp.pagenum;
                            wb.Navigate(string.Format("http://{1}.3gokushi.jp/card/deck.php?l={0}&p={2}"
                                , asp.tabnum.ToString(), worldid.Text, asp.pagenum.ToString()));
                        }
                        else
                        {
                            //次のスキルへ
                            if (asp.status != ZG_ID) { jsIdx++; }
                        }
                        return;
                    }
                }
                else
                {
                    //自動ログイン
                    if (w.Document.All.GetElementsByName("email").Count > 0)
                    {
                        w.Document.All.GetElementsByName("email")[0].InnerText = loginid.Text;
                        w.Document.All.GetElementsByName("password")[0].InnerText = loginpw.Text;
                        w.Document.Forms[0].InvokeMember("submit");
                    }

                }
            }
            else
            {
                //読み込み未完了時の処理
                Console.Write(e.Url);
            }
        }

        private int GetCurrentPagdenum()
        {
            if (curPagenum.ContainsKey(asp.skillname))
            {
                if (curPagenum[asp.skillname] <= asp.maxpagenum)
                {
                    debug_log(string.Format("スキル：{0} Page:{1}", asp.skillname, curPagenum[asp.skillname]));
                    return curPagenum[asp.skillname];
                }
            }
            else
            {
                curPagenum.Add(asp.skillname, 1);
            }
            debug_log(string.Format("スキル：{0} Page:{1}", asp.skillname, curPagenum[asp.skillname]));
            return 1;
        }

        const string _gsp_default = "gspdefault";
        private Point GetSyuppeiPoint(string bname = _gsp_default)
        {
            //固定出兵チェック
            if (!_gsp_default.Equals(bname))
            {
                foreach (Config.mpb mp in this.getMpList())
                {
                    //目標判定
                    bool skip = false;
                    if (mp.type.Equals("木") && decimal.ToInt32(woodm.Value) - wood < 0) { skip = true; }
                    if (mp.type.Equals("石") && decimal.ToInt32(stonem.Value) - stone < 0) { skip = true; }
                    if (mp.type.Equals("鉄") && decimal.ToInt32(ironm.Value) - iron < 0) { skip = true; }
                    if (mp.type.Equals("糧") && decimal.ToInt32(ricem.Value) - rice < 0) { skip = true; }

                    if (bname.Trim().Equals(mp.name.Trim()) && true == mp.flag
                        && !skip)
                    {
                        mtype = "固定";
                        return new Point(mp.x, mp.y);
                    }
                }
            
            }

            //資源目標との差分が大きいほど出兵しやすくなるようにする
            //順に 4/10 3/10 2/10 1/10
            SortedList<int, int> list = new SortedList<int, int>();
            int dis = 0;

            dis = decimal.ToInt32(woodm.Value) - wood;
            if (dis > 0 && !list.ContainsKey(dis) && woodF.Checked) { list.Add(dis, 0); }

            dis = decimal.ToInt32(stonem.Value) - stone;
            if (dis > 0 && !list.ContainsKey(dis) && stoneF.Checked) { list.Add(dis, 1); }

            dis = decimal.ToInt32(ironm.Value) - iron;
            if (dis > 0 && !list.ContainsKey(dis) && ironF.Checked) { list.Add(dis, 2); }

            dis = decimal.ToInt32(ricem.Value) - rice;
            if (dis > 0 && !list.ContainsKey(dis) && riceF.Checked) { list.Add(dis, 3); }

            //資源目標達成時の動作
            if (0 == list.Count && overCapaChk.Checked)
            {
                dis = res - wood; if (dis > 0 && !list.ContainsKey(dis)) { list.Add(dis, 0); }
                dis = res - stone; if (dis > 0 && !list.ContainsKey(dis)) { list.Add(dis, 1); }
                dis = res - iron; if (dis > 0 && !list.ContainsKey(dis)) { list.Add(dis, 2); }
                dis = res - rice; if (dis > 0 && !list.ContainsKey(dis)) { list.Add(dis, 3); }
            }

            int c = 0;
            List<int> ptindex = new List<int>();
            foreach(var item in list){
                c++;
                for (int i = 0; i < c; i++)
                {
                    ptindex.Add(item.Value);
                }
            }
            
            Random r = new System.Random(Environment.TickCount);
            for (int i = 0; i <= 1000; i++)
            {
               if (0 == ptindex.Count) { continue; }
               int idx = ptindex[r.Next() % ptindex.Count];
                
               switch (idx)
                {
                    case 0:
                        {  mtype = "木";return new Point(Decimal.ToInt32(wood_x.Value),Decimal.ToInt32(wood_y.Value)); }
                        goto ExitLoop;
                    case 1:                        
                        { mtype = "石";return new Point(Decimal.ToInt32(stone_x.Value), Decimal.ToInt32(stone_y.Value)); }
                        goto ExitLoop;
                    case 2:                        
                        { mtype = "鉄";return new Point(Decimal.ToInt32(iron_x.Value), Decimal.ToInt32(iron_y.Value)); }
                        goto ExitLoop;
                    case 3:
                        { mtype = "糧"; return new Point(Decimal.ToInt32(rice_x.Value), Decimal.ToInt32(rice_y.Value)); }
                        goto ExitLoop;
                    default:
                        break;
                }
            }
        ExitLoop: ;

            mtype = "--";
            log("出兵先不明");
            return new Point(999,999);
        }

        private string GetSkillVillageId(bool skillvillageflg = true)
        {
            HtmlElement vlist = wb.Document.GetElementById("deck_add_selected_village");
            if (vlist != null)
            {
                foreach (HtmlElement op in vlist.GetElementsByTagName("option"))
                {
                    if (true == skillvillagechk.Checked && skillvillageflg)
                    {
                        if (op.InnerHtml.Equals(skillvillages.Text))
                        {
                            return op.GetAttribute("value");
                        }
                    }
                    else
                    {
                        if (op.InnerHtml.Equals(villages.Text))
                        {
                            return op.GetAttribute("value");
                        }
                    }
                }
            }
            return string.Empty;
        }

        private List<Busyo> GetBusyoList(WebBrowser web) {
            List<Busyo> list = new List<Busyo>();

            foreach (HtmlElement el in web.Document.GetElementsByTagName("div"))
            {
                if (!el.GetAttribute("className").Equals("bushoList")) { continue; }
                //debug_log(el.InnerHtml);

                //int cnt = 0;
                foreach (HtmlElement be in el.GetElementsByTagName("table"))
                {
                    if (!be.GetAttribute("className").Equals("general attackGeneralListTbl")) { continue; }
                   //debug_log(be.InnerHtml);
                    //if (cnt++ <= 0) { continue; }

                    int c = 0;
                    Busyo b = new Busyo();
                    foreach (HtmlElement tr in be.GetElementsByTagName("tr"))
                    {
                        c++;
                        if (c <= 1) { continue; }
                       
                        debug_log(tr.InnerHtml);

                        HtmlElementCollection td = tr.GetElementsByTagName("td");
                        if (c == 2)
                        {
                            //sentaku
                            b.Status = td[0].InnerHtml.Trim();

                            //id
                            HtmlElementCollection r = td[0].GetElementsByTagName("input");
                            if (r.Count > 0)
                            {
                                b.Id = r[0].GetAttribute("id");
                            }

                            //name
                            b.Name = td[1].GetElementsByTagName("a")[1].InnerHtml;

                            //tobatu
                            string tob="<STRONG>討</STRONG>";
                            string html = td[2].InnerHtml.ToUpper().Replace("\t", string.Empty);
                            html = html.Replace("<SPAN CLASS=\"RED\">", string.Empty).Replace("</SPAN>", string.Empty);
                            string buf = html.Substring(html.IndexOf(tob) + tob.Length);

                            string atk = "<STRONG>攻撃</STRONG>";
                            buf = buf.Substring(0, buf.IndexOf(atk)).Trim();

                            //debug_log(buf);
                            b.tobatu = int.Parse(buf.Substring(0, buf.IndexOf("</P>")).Trim());

                            //attack
                            buf = html.Substring(html.IndexOf(atk) + atk.Length);

                            string tiryo = "<STRONG>知力</STRONG>";
                            buf = buf.Substring(0, buf.IndexOf(tiryo) - 1).Trim();
                            b.attack = int.Parse(this.redParam(buf));

                            //speed
                            string spd = "<STRONG>速度</STRONG>";
                            buf = html.Substring(html.IndexOf(spd) + spd.Length);

                            string spde = "<BR>\n<STRONG>防御力</STRONG>";
                            buf = buf.Substring(0, buf.IndexOf(spde) - 1).Trim();
                            b.speed = (int)double.Parse(this.redParam(buf));
                            

                            //hp
                            string hp = "<STRONG>HP</STRONG>";
                            buf = html.Substring(html.IndexOf(hp) + hp.Length);

                            buf = buf.Substring(0, buf.IndexOf(string.Format("&NBSP;\n{0}", tob))).Trim();
                            b.hp = int.Parse(buf);

                            debug_log(string.Format("武将情報-------------------------------"));
                            debug_log(string.Format("武将名：{0}",b.Name));
                            debug_log(string.Format("討伐：{0}", b.tobatu));
                            debug_log(string.Format("攻撃力：{0}", b.attack));
                            debug_log(string.Format("速度：{0}", b.speed));
                            debug_log(string.Format("ＨＰ：{0}", b.hp));
                            debug_log(string.Format("ID：{0}", b.Id));
                            debug_log(string.Format("スキルID：{0}", b.skillId));
                            debug_log(string.Format("スキル名：{0}", b.skillName));
                            debug_log(string.Format("ステータス：{0}", b.Status));
                            foreach (string msg in b.skill)
                            {
                                debug_log(string.Format("スキル：{0}", msg));
                            }
                            


                        }
                    }

                    //skill
                    string key = "skill_radio_";
                    foreach (HtmlElement td in be.GetElementsByTagName("td"))
                    {
                        foreach (HtmlElement inp in td.GetElementsByTagName("input"))
                        {
                            if (inp.GetAttribute("id").Substring(0, key.Length).Equals(key))
                            {
                                foreach (HtmlElement a in td.GetElementsByTagName("a"))
                                {
                                    b.skillId = inp.GetAttribute("id");
                                    b.skillName = a.InnerHtml;
                                    goto exitloop;
                                }
                            }
                        }

                        int idx = td.InnerHtml.IndexOf("LV");
                        if (idx > 0)
                        {
                            b.skill.Add(td.InnerHtml.Substring(0, idx));
                        }
                    }
                exitloop:;


                    list.Add(b);
                }
                
            }
            
            return list;
        }

        private void RunSyuppei(){
            rflg = true;
            wb.Navigate(string.Format("http://{0}.3gokushi.jp/facility/castle_send_troop.php", worldid.Text));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timerflg = !timerflg;
            if (timerflg)
            {
                runbtn.Text = "自動出兵中止";
                timer.Enabled = true;

                //diff base reset
                base_wood = 0;
                base_stone = 0;
                base_iron = 0;
                base_rice = 0;
            }
            else
            {
                runbtn.Text = "自動出兵開始";
                timercount = int.Parse(spannum.Value.ToString());
                timer.Enabled = false;
            }           
        }

        private string redParam(string s){
            double d;
            if (!double.TryParse(s, out d))
            {
                s = s.Replace("<SPAN class=red>", string.Empty);
                s = s.Replace("</SPAN>", string.Empty);
            }
            return s;
        }

        private bool CheckVillage()
        {
            bool ret = false;
            string curvillage = villages.Text;
            villages.Items.Clear();
            string curskillvillage = skillvillages.Text;
            skillvillages.Items.Clear();
            foreach (HtmlElement el in wb.Document.GetElementsByTagName("td"))
            {
                if (el.GetAttribute("className").Equals("facilityRollOver"))
                {
                    foreach (HtmlElement el2 in el.GetElementsByTagName("label"))
                    {
                        string lab = el2.InnerHtml.Substring(el2.InnerHtml.LastIndexOf(">") + 1).Trim();
                        villages.Items.Add(lab);
                        skillvillages.Items.Add(lab);
                        if (lab.Equals(curvillage))
                        {
                            foreach (HtmlElement el3 in el2.GetElementsByTagName("input"))
                            {
                                if (el3.GetAttribute("id").Equals("village"))
                                {
                                    el3.InvokeMember("click");
                                    ret = true;
                                    log(string.Format("拠点変更：{0}", curvillage));
                                }
                            }
                        }
                        
                    }
                }
            }
            villages.Text = curvillage;
            skillvillages.Text = curskillvillage;
            return ret;
        }

        private void SetCurrentResource()
        {
            HtmlElement max = wb.Document.GetElementById("wood_max");
            if (max!=null) {
                woodl.Text = string.Format("木: {0} / {1}", wb.Document.GetElementById("wood").InnerHtml.ToString(), max.InnerHtml.ToString());
                stonel.Text = string.Format("石: {0} / {1}", wb.Document.GetElementById("stone").InnerHtml.ToString(), max.InnerHtml.ToString());
                ironl.Text = string.Format("鉄: {0} / {1}", wb.Document.GetElementById("iron").InnerHtml.ToString(), max.InnerHtml.ToString());
                ricel.Text = string.Format("糧: {0} / {1}", wb.Document.GetElementById("rice").InnerHtml.ToString(), max.InnerHtml.ToString());

                wood = int.Parse(wb.Document.GetElementById("wood").InnerHtml.ToString());
                stone = int.Parse(wb.Document.GetElementById("stone").InnerHtml.ToString());
                iron = int.Parse(wb.Document.GetElementById("iron").InnerHtml.ToString());
                rice = int.Parse(wb.Document.GetElementById("rice").InnerHtml.ToString());
                res = int.Parse(max.InnerHtml.ToString());

                woodm.Maximum = int.Parse(max.InnerHtml.ToString());
                stonem.Maximum = int.Parse(max.InnerHtml.ToString());
                ironm.Maximum = int.Parse(max.InnerHtml.ToString());
                ricem.Maximum = int.Parse(max.InnerHtml.ToString());

                if (base_wood + base_stone + base_iron + base_rice == 0)
                {
                    base_wood = wood;
                    base_stone = stone;
                    base_iron = iron;
                    base_rice = rice;
                }

                
                diff.Text = string.Format("【取得資源量】　木：{0}　石：{1}　鉄：{2}　糧：{3}"//　　【使用スキル】　{4}:P{5}　傾国:P{6}"
                    , wood - base_wood, stone - base_stone, iron - base_iron, rice - base_rice
                    //, jsList[jsIdx], GetCurrentPagdenum(), 0
                    );     
                
                List<string> jsList = this.getJinkunSkillList();
                int page1 = 1; int page2 = 1;
                if (curPagenum.ContainsKey(jsList[jsIdx])) { page1 = curPagenum[jsList[jsIdx]]; }
                if (curPagenum.ContainsKey("傾国")) { page2 = curPagenum["傾国"]; }
                tsSkill.Text = string.Format("【使用スキル】　{0}:Pgage {1}　傾国:Page {2}"
                    , jsList[jsIdx], page1, page2); ;
            }

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            --timercount;
            if (timercount <= 0)
            {
                asp.status = 0;
                if (timerflg) { this.RunSyuppei(); }
                timercount = int.Parse(spannum.Value.ToString());
            }
            stsmsg.Text = string.Format("次回出兵まであと：{0}秒", timercount);
        }

        private void debug_log(string msg)
        {
            if (cbdebug.Checked) { log(msg); }
        }

        private void log(string msg)
        {
            logbox.Text += string.Format("{0} {1}{2}", DateTime.Now.ToString()
                , msg.Replace("\t", string.Empty).Replace("\n", string.Empty), System.Environment.NewLine);
            int max =20000;
            if (logbox.Text.Length > max)
            {
                logbox.Text = logbox.Text.Substring(logbox.Text.Length - max, max);
            }
            logbox.SelectionStart = logbox.Text.Length;
            logbox.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            try
            {
                //保存先のファイル名
                string fileName = this.config_path();

                //XmlSerializerオブジェクトを作成
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                Config obj = (Config)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();

                loginid.Text = obj.userid;
                loginpw.Text = obj.userpasswd;
                if (true == TOOL_MODE)
                {
                    worldid.Text = obj.loginworld;
                }

                toubatu.Value = obj.condition;
                toubatuChk.Checked = obj.condition_flag;

                attackBorder.Value = obj.attack;
                try{
                    speedBorder.Value = obj.speed;
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                spannum.Value = obj.timer_span;

                foreach (string s in obj.village_list)
                {
                    villages.Items.Add(s);
                    skillvillages.Items.Add(s);
                }
                villages.Text = obj.village;

                try{
                    skillvillagechk.Checked = obj.skillvillage_flag;
                    skillvillages.Text = obj.skillvillage;

                    overCapaChk.Checked = obj.overres_flag;
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                chkakiti.Checked = obj.pointtypecheck_flag;

                chkKeikoku.Checked = obj.keikoku_flag;
                keikokutab.Value = obj.keikoku_tabnum;
                keikokuPageNum.Value = obj.keikoku_pagenum;

                try
                {
                    chkjinkun.Checked = obj.jinkun_flag;
                    jinkunhp.Value = obj.jinkunhp;
                    jinkuntab.Value = obj.jinkun_tabnum;
                    jinkunpage.Value = obj.jinkun_pagenum;
                }
                catch (Exception ex) { this.debug_log(ex.Message); }
                try{
                    if (null!=obj.jinkun_skill_list)
                    {
                        jinkunSkillList.Text = obj.jinkun_skill_list.Replace("\n", "\r\n");
                    }
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                woodm.Value = obj.wood_max;
                wood_x.Value = obj.wood_x;
                wood_y.Value = obj.wood_y;

                stonem.Value = obj.stone_max;
                stone_x.Value = obj.stone_x;
                stone_y.Value = obj.stone_y;

                ironm.Value = obj.iron_max;
                iron_x.Value = obj.iron_x;
                iron_y.Value = obj.iron_y;

                ricem.Value = obj.rice_max;
                rice_x.Value = obj.rice_x;
                rice_y.Value = obj.rice_y;

                try
                {
                    woodF.Checked = obj.wood_F;
                    stoneF.Checked = obj.stone_F;
                    ironF.Checked = obj.iron_F;
                    riceF.Checked = obj.rice_F;
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                try
                {
                    zgFlg.Checked = obj.zg_flg;
                    zgNo.Value = obj.zg_no;
                    zgName.Text = obj.zg_name;
                    zgTab.Value = obj.zg_tab;
                    zgPage.Value = obj.zg_page;
                    szbdlg.zgblist.Clear();
                    foreach (SelectZengunBusyo.zgb zbd in obj.zg_list)
                    {
                        szbdlg.zgblist.Add(zbd);
                    }
                    szbdlg.setgrid();
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                try
                {
                    mpgv.Rows.Clear();
                    foreach (Config.mpb mp in obj.mp_list)
                    {
                        mpgv.Rows.Add();
                        mpgv.Rows[mpgv.Rows.Count - 2].Cells[0].Value = mp.flag;
                        mpgv.Rows[mpgv.Rows.Count - 2].Cells[1].Value = mp.name.Trim();
                        mpgv.Rows[mpgv.Rows.Count - 2].Cells[2].Value = mp.x.ToString();
                        mpgv.Rows[mpgv.Rows.Count - 2].Cells[3].Value = mp.y.ToString();
                        mpgv.Rows[mpgv.Rows.Count - 2].Cells[4].Value = mp.type.ToString();                        
                    }
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

                try
                {
                    if (null!=obj.kd_skill && !string.Empty.Equals(obj.kd_skill))
                    {
                        koudatuList.Text = obj.kd_skill.Replace("\n", "\r\n");
                    }
                    koudatuHP.Checked = obj.kd_hp;
                }
                catch (Exception ex) { this.debug_log(ex.Message); }

            }
            catch (Exception ex)
            {
                debug_log(ex.Message);
                log("設定の読み込みに失敗しました");
            }
            finally
            {
            }
    
        }

        private void woodm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            woodm.Value = res;
        }

        private void stonem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            stonem.Value = res;
        }

        private void ironm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ironm.Value = res;
        }

        private void ricem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ricem.Value = res;
        }

        private string config_path() { return string.Format("{0}.conf", Assembly.GetEntryAssembly().Location); }
       
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                //保存先のファイル名
                string fileName = this.config_path();

                //保存するクラス(SampleClass)のインスタンスを作成
                Config obj = new Config();

                obj.userid = loginid.Text;
                obj.userpasswd = loginpw.Text;
                obj.loginworld = worldid.Text;

                obj.condition = decimal.ToInt32(toubatu.Value);
                obj.condition_flag = toubatuChk.Checked;

                obj.attack = decimal.ToInt32(attackBorder.Value);
                obj.speed = decimal.ToInt32(speedBorder.Value);

                obj.timer_span = decimal.ToInt32(spannum.Value);

                obj.village = villages.Text;
                obj.village_list = new List<string>();
                foreach(Object o in villages.Items)
                {
                    obj.village_list.Add(o.ToString());
                }

                obj.pointtypecheck_flag = chkakiti.Checked;

                obj.keikoku_flag = chkKeikoku.Checked;
                obj.keikoku_tabnum = decimal.ToInt32(keikokutab.Value);
                obj.keikoku_pagenum = decimal.ToInt32(keikokuPageNum.Value);

                obj.jinkun_flag = chkjinkun.Checked;
                obj.jinkunhp = decimal.ToInt32(jinkunhp.Value);
                obj.jinkun_tabnum = decimal.ToInt32(jinkuntab.Value);
                obj.jinkun_pagenum = decimal.ToInt32(jinkunpage.Value);
                obj.jinkun_skill_list = jinkunSkillList.Text;

                obj.skillvillage_flag = skillvillagechk.Checked;
                obj.skillvillage = skillvillages.Text;

                obj.overres_flag = overCapaChk.Checked;

                obj.wood_max = decimal.ToInt32(woodm.Value);
                obj.wood_F = woodF.Checked;
                obj.wood_x = decimal.ToInt32(wood_x.Value);
                obj.wood_y = decimal.ToInt32(wood_y.Value);

                obj.stone_max = decimal.ToInt32(stonem.Value);
                obj.stone_F = stoneF.Checked;
                obj.stone_x = decimal.ToInt32(stone_x.Value);
                obj.stone_y = decimal.ToInt32(stone_y.Value);

                obj.iron_max = decimal.ToInt32(ironm.Value);
                obj.iron_F = ironF.Checked;
                obj.iron_x = decimal.ToInt32(iron_x.Value);
                obj.iron_y = decimal.ToInt32(iron_y.Value);

                obj.rice_max = decimal.ToInt32(ricem.Value);
                obj.rice_F = riceF.Checked;
                obj.rice_x = decimal.ToInt32(rice_x.Value);
                obj.rice_y = decimal.ToInt32(rice_y.Value);

                obj.zg_flg = zgFlg.Checked;
                obj.zg_no = decimal.ToInt32(zgNo.Value);
                obj.zg_name = zgName.Text;
                obj.zg_tab = decimal.ToInt32(zgTab.Value);
                obj.zg_page = decimal.ToInt32(zgPage.Value);
                obj.zg_list = szbdlg.zgblist;


                obj.mp_list = this.getMpList();

                obj.kd_skill = koudatuList.Text;
                obj.kd_hp = koudatuHP.Checked;

                    //save
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
                serializer.Serialize(sw, obj);
                sw.Close();

                MessageBox.Show("設定を保存しました。");

            }
            catch (Exception ex)
            {
                debug_log(ex.Message);
                log("設定の保存に失敗しました");
            }
            finally
            {
            }
    
        }

        private List<Config.mpb> getMpList()
        {
            List<Config.mpb> mplist = new List<Config.mpb>();
            foreach (DataGridViewRow row in mpgv.Rows)
            {
                Config.mpb mp = new Config.mpb();
                int ibuf;

                try
                {
                    mp.flag = (bool)row.Cells[0].Value;
                    mp.name = row.Cells[1].Value.ToString().Trim();
                    mp.x = 999;
                    if (int.TryParse(row.Cells[2].Value.ToString(), out ibuf))
                    {
                        mp.x = ibuf;
                    }

                    mp.y = 999;
                    if (int.TryParse(row.Cells[3].Value.ToString(), out ibuf))
                    {
                        mp.y = ibuf;
                    }

                    mp.type = string.Empty;
                    if (null != row.Cells[4].Value)
                    {
                        mp.type = row.Cells[4].Value.ToString();
                    }
                    mplist.Add(mp);
                }
                catch (Exception ex) { }
            }
            return mplist;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.button1_Click_1(sender, e);
        }

        private void jinkunSkillList_Leave(object sender, EventArgs e)
        {
            List<string> list;
            list = this.getJinkunSkillList();

            jinkunSkillList.Text = string.Empty;
            foreach (string l in list)
            {
                jinkunSkillList.Text += l + System.Environment.NewLine; 
            }
        }

        private List<string> getJinkunSkillList()
        {
            List<string> list = new List<string>();

            string buf = jinkunSkillList.Text;

            buf = buf.Replace(Convert.ToChar(9).ToString(),string.Empty); //\t
            for (int i = 32; i < 127; i++)
            {
                buf = buf.Replace(Convert.ToChar(i).ToString(),string.Empty);
            }

            string[] lines = buf.Split(new string[]{System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<lines.Length;i++){
                list.Add(lines[i].Trim());
            }

            if (list.Count <= 0) { list.Add("仁君"); }

            return list;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void setColor()
        {
            if (woodF.Checked) { woodm.BackColor = Color.White; } else { woodm.BackColor = Color.Silver; }
            if (stoneF.Checked) { stonem.BackColor = Color.White; } else { stonem.BackColor = Color.Silver; }
            if (ironF.Checked) { ironm.BackColor = Color.White; } else { ironm.BackColor = Color.Silver; }
            if (riceF.Checked) { ricem.BackColor = Color.White; } else { ricem.BackColor = Color.Silver; }
        }

        private void woodF_CheckedChanged(object sender, EventArgs e)
        {
            this.setColor();            
        }

        private void stoneF_CheckedChanged(object sender, EventArgs e)
        {
            this.setColor();
        }

        private void ironF_CheckedChanged(object sender, EventArgs e)
        {
            this.setColor();
        }

        private void riceF_CheckedChanged(object sender, EventArgs e)
        {
            this.setColor();
        }

        private void skillreset_Click(object sender, EventArgs e)
        {
            jsIdx = 0;
            foreach (KeyValuePair<string, int> cp in curPagenum)
            {
                try
                {
                    if (curPagenum.ContainsKey(cp.Key))
                    {
                        curPagenum[cp.Key] = 1;
                    }
                }
                catch (Exception ex)
                {
                    debug_log(ex.Message);
                }
            }
            MessageBox.Show("回復スキルとページング情報を初期化しました");
        }

        private List<string> getKoudatuList()
        {
            List<string> list = new List<string>();

            string buf = koudatuList.Text.Trim();

            buf = buf.Replace(Convert.ToChar(9).ToString(),string.Empty); //\t
            for (int i = 32; i < 127; i++)
            {
                buf = buf.Replace(Convert.ToChar(i).ToString(),string.Empty);
            }

            string[] lines = buf.Split(new string[]{System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<lines.Length;i++){
                list.Add(lines[i].Trim());
            }

            return list;
        }

        
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (DialogResult.OK== szbdlg.ShowDialog())
            {
                foreach (SelectZengunBusyo.zgb _b in szbdlg.zgblist)
                {
                    if (_b.chk)
                    {
                        zgNo.Value = _b.id;
                        zgName.Text = _b.name;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes
                == MessageBox.Show("ツールが使用するブラウザの動作バージョンをチェックしますか？外部サイトへ接続します。","外部サイトへの接続確認", MessageBoxButtons.YesNo))
            {
                wb.Navigate("http://www.useragentstring.com/");
                tabControl1.SelectedIndex = 0;
            }
        }

        private void mpgv_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                DataGridView dgv = (DataGridView)sender;
                DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;

                tb.KeyPress -= new KeyPressEventHandler(dataGridViewTextBox_KeyPress);

                if (dgv.CurrentCell.OwningColumn.Name == "Column3" || dgv.CurrentCell.OwningColumn.Name == "Column4")
                {
                    tb.KeyPress += new KeyPressEventHandler(dataGridViewTextBox_KeyPress);
                }
            }
        }

        private void dataGridViewTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            if ((e.KeyChar >= '0' && e.KeyChar <= '9')
                || e.KeyChar == '-'
                || e.KeyChar == '\b'
                )
            {
                e.Handled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.button1_Click_1(sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in mpgv.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    mpgv.Rows.Remove(r);
                }
            }
        }

        private void wb_NewWindow3(object sender, WebBrowserNewWindow3EventArgs e)
        {
            // IEでウインドウを開くのを抑止
            e.Cancel = true;
            wb.Navigate(e.bstrUrl);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            wb.Navigate(string.Format("http://{0}.3gokushi.jp/land.php?x={1}&y={2}"
                                , worldid.Text, decimal.ToInt32(wood_x.Value), decimal.ToInt32(wood_y.Value)));
            tabControl1.SelectedIndex = 0;
                
        }

        private void button8_Click(object sender, EventArgs e)
        {
            wb.Navigate(string.Format("http://{0}.3gokushi.jp/land.php?x={1}&y={2}"
                               , worldid.Text, decimal.ToInt32(stone_x.Value), decimal.ToInt32(stone_y.Value)));
            tabControl1.SelectedIndex = 0;
                
        }

        private void button9_Click(object sender, EventArgs e)
        {
            wb.Navigate(string.Format("http://{0}.3gokushi.jp/land.php?x={1}&y={2}"
                               , worldid.Text, decimal.ToInt32(iron_x.Value), decimal.ToInt32(iron_y.Value)));
            tabControl1.SelectedIndex = 0;
                
        }

        private void button10_Click(object sender, EventArgs e)
        {
            wb.Navigate(string.Format("http://{0}.3gokushi.jp/land.php?x={1}&y={2}"
                               , worldid.Text, decimal.ToInt32(rice_x.Value), decimal.ToInt32(rice_y.Value)));
            tabControl1.SelectedIndex = 0;
                
        }


    }
}
