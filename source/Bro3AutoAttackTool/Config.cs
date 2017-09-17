using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bro3AutoAttackTool
{
    public class Config
    {
        public string userid;
        public string userpasswd;
        public string loginworld;

        public int condition;
        public bool condition_flag;

        public int attack;
        public int speed;

        public int timer_span;

        public string village;
        public List<string> village_list;

        public bool pointtypecheck_flag;

        public bool keikoku_flag;
        public int keikoku_tabnum;
        public int keikoku_pagenum;

        public bool jinkun_flag;
        public int jinkunhp;
        public int jinkun_tabnum;
        public int jinkun_pagenum;
        public string jinkun_skill_list;

        public bool skillvillage_flag;
        public string skillvillage;

        public bool overres_flag;

        public int wood_max;
        public bool wood_F;
        public int wood_x;
        public int wood_y;

        public int stone_max;
        public bool stone_F;
        public int stone_x;
        public int stone_y;

        public int iron_max;
        public bool iron_F;
        public int iron_x;
        public int iron_y;

        public int rice_max;
        public bool rice_F;
        public int rice_x;
        public int rice_y;

        public bool zg_flg;
        public int zg_no;
        public string zg_name;
        public int zg_tab;
        public int zg_page;
        public List<SelectZengunBusyo.zgb> zg_list;

        public struct mpb
        {
            public bool flag;
            public string name;
            public int x;
            public int y;
            public string type;
        }
        public List<mpb> mp_list;



        public string kd_skill;
        public bool kd_hp;
    }
}
