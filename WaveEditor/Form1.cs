using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using Newtonsoft.Json;

namespace WaveEditor
{
    public struct s_settings
    {
        public string settingName;
        public string value;
        public string valueType;
    }

    public struct s_condition
    {
        public string conditionName;
        public string conditionIndex;
        public string value;
    }

    public struct s_effect
    {
        public string effectName;
        public List<s_settings> effectSettings;
        public List<s_condition> conditions;
    }

    public struct s_instance
    {
        public string enemy;
        public List<s_settings> enemySettings;
        public List<s_effect> effects;
        public decimal delay;
        public List<int> pos;
    }


    public partial class Form1 : Form
    {
        private string enemyPfade = "\\Enemy\\Enemys";
        private List<s_instance> instances = new List<s_instance>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button_LoadGame_Click(object sender, EventArgs e)
        {
            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo(textBox_GamePfade.Text + "\\Enemy\\Enemys");

            foreach (System.IO.FileInfo f in ParentDirectory.GetFiles())
            {
                comboBox_Enemys.Items.Add(f.Name);
            }
            comboBox_Enemys.SelectedIndex = 0;

            ParentDirectory = new System.IO.DirectoryInfo(textBox_GamePfade.Text + "\\effects");

            foreach (System.IO.FileInfo f in ParentDirectory.GetFiles())
            {
                if (f.Name != "Tags.tscn" && f.Name != "StatusEffect.tscn")
                {
                    comboBox_Effects.Items.Add(f.Name);
                }
            }
            comboBox_Effects.SelectedIndex = 0;

            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(textBox_GamePfade.Text + "\\effects\\scripts\\StatusEffect.gd");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("enum e_condition{"))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains("}")) break;
                        comboBox_EffectsCondtion.Items.Add(line.Trim());
                    }
                }
            }
            comboBox_EffectsCondtion.SelectedIndex = 0;


        }

        private void button_AddEnemy_Click(object sender, EventArgs e)
        {
            if (comboBox_Enemys.SelectedItem != null)
            {
                s_instance inc = new s_instance();
                inc.enemy = comboBox_Enemys.SelectedItem.ToString();
                listBox_Enemys.Items.Add(inc.enemy);
                inc.effects = new List<s_effect>();
                inc = addSettingsToInstance(inc);
                instances.Add(inc);
            }


        }

        private void button_Delete_Enemy_Click(object sender, EventArgs e)
        {
            if (listBox_Enemys.SelectedItems.Count > 0)
            {
                int index = listBox_Enemys.SelectedIndex;
                instances.RemoveAt(index);
                listBox_Enemys.Items.Remove(listBox_Enemys.SelectedItem);
                textBox_EnemysSettingsValueType.Text = "";
                textBox_EnemysSettingsValue.Text = "";
                button_SaveEnemysSettings.Enabled = false;
                button_AddEffect.Enabled = false;
            }
        }

        private void listBox_Enemys_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox_EnemysSettings.Items.Clear();
            if (listBox_Enemys.SelectedIndex >= 0 && listBox_Enemys.SelectedIndex < instances.Count)
            {
                numericUpDown_Delay.Value = instances[listBox_Enemys.SelectedIndex].delay;
                numericUpDown_POSx.Value = (instances[listBox_Enemys.SelectedIndex].pos != null) ? instances[listBox_Enemys.SelectedIndex].pos[0] : 0;
                numericUpDown_POSy.Value = (instances[listBox_Enemys.SelectedIndex].pos != null) ? instances[listBox_Enemys.SelectedIndex].pos[1] : 0;

                if (instances[listBox_Enemys.SelectedIndex].enemySettings != null)
                {
                    foreach (var setting in instances[listBox_Enemys.SelectedIndex].enemySettings)
                    {
                        listBox_EnemysSettings.Items.Add(setting.settingName + (setting.value != "" && setting.value != null ? " (" + setting.value + ")" : ""));
                    }
                }
            }
            listBox_Effects.Items.Clear();
            listBox_EffectsSettings.Items.Clear();
            textBox_EffectsSettingsValue.Text = "";
            textBox_EffectsSettingsValueType.Text = "";
            button_SaveEffectsSettings.Enabled = false;
            if (listBox_Enemys.SelectedIndex >= 0 && listBox_Enemys.SelectedIndex < instances.Count)
            {
                if (instances[listBox_Enemys.SelectedIndex].enemySettings != null)
                {
                    foreach (var effect in instances[listBox_Enemys.SelectedIndex].effects)
                    {
                        listBox_Effects.Items.Add(effect.effectName);
                    }
                }
                button_AddEffect.Enabled = true;
            }

        }

        private void listBox_EnemysSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_EnemysSettings.SelectedIndex >= 0 && listBox_EnemysSettings.SelectedIndex < instances[listBox_Enemys.SelectedIndex].enemySettings.Count)
            {
                textBox_EnemysSettingsValueType.Text = instances[listBox_Enemys.SelectedIndex].enemySettings[listBox_EnemysSettings.SelectedIndex].valueType;
                textBox_EnemysSettingsValue.Text = instances[listBox_Enemys.SelectedIndex].enemySettings[listBox_EnemysSettings.SelectedIndex].value;
                button_SaveEnemysSettings.Enabled = true;
            }

        }

        private void button_SaveEnemysSettings_Click(object sender, EventArgs e)
        {
            if (listBox_EnemysSettings.SelectedIndex == -1) return;
            s_settings set = instances[listBox_Enemys.SelectedIndex].enemySettings[listBox_EnemysSettings.SelectedIndex];
            instances[listBox_Enemys.SelectedIndex].enemySettings.Remove(instances[listBox_Enemys.SelectedIndex].enemySettings[listBox_EnemysSettings.SelectedIndex]);
            listBox_EnemysSettings.Items.RemoveAt(listBox_EnemysSettings.SelectedIndex);
            set.value = textBox_EnemysSettingsValue.Text;
            textBox_EnemysSettingsValue.Text = "";
            instances[listBox_Enemys.SelectedIndex].enemySettings.Add(set);
            listBox_EnemysSettings.Items.Add(set.settingName + (set.value != "" && set.value != null ? " (" + set.value + ")" : ""));
        }

        private void button_AddEffect_Click(object sender, EventArgs e)
        {
            if (comboBox_Effects.SelectedItem != null)
            {
                s_effect effect = new s_effect();
                effect.effectName = comboBox_Effects.SelectedItem.ToString();
                listBox_Effects.Items.Add(effect.effectName);
                effect = addSettingsToEffect(effect);
                effect.conditions = new List<s_condition>();
                instances[listBox_Enemys.SelectedIndex].effects.Add(effect);
            }
        }

        private void listBox_Effects_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox_EffectsSettings.Items.Clear();
            textBox_EffectsSettingsValue.Text = "";
            textBox_EffectsSettingsValueType.Text = "";
            button_SaveEffectsSettings.Enabled = false;

            listBox_EffectsCondition.Items.Clear();
            textBox_EffectsCondtionsValue.Text = "";
            button_AddEffectsCondition.Enabled = false;
            if (listBox_Effects.SelectedIndex >= 0 && listBox_Effects.SelectedIndex < instances[listBox_Enemys.SelectedIndex].effects.Count)
            {
                if (instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings != null)
                {
                    foreach (var setting in instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings)
                    {
                        listBox_EffectsSettings.Items.Add(setting.settingName + (setting.value != "" && setting.value != null ? " (" + setting.value + ")" : ""));
                    }
                }
                button_AddEffectsCondition.Enabled = true;
                if (instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions != null)
                {
                    foreach (var conditions in instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions)
                    {
                        listBox_EffectsCondition.Items.Add(conditions.conditionName + (conditions.value != "" && conditions.value != null ? " (" + conditions.value + ")" : ""));
                    }
                }
            }
        }

        private s_instance addSettingsToInstance(s_instance _inc)
        {
            string line;

            _inc.enemySettings = new List<s_settings>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(textBox_GamePfade.Text + "\\Enemy\\Enemy.gd");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("export"))
                {
                    s_settings set = new s_settings();
                    set.valueType = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - (line.IndexOf('(') + 1));
                    int varEnd = line.IndexOf("var") + 4;
                    set.settingName = line.Substring(varEnd, (line.IndexOf(' ', varEnd) > 0 ? line.IndexOf(' ', varEnd) : line.Length) - varEnd);
                    _inc.enemySettings.Add(set);
                }
            }

            file.Close();

            return _inc;
        }

        private s_effect addSettingsToEffect(s_effect _effect)
        {
            string line;

            _effect.effectSettings = new List<s_settings>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(textBox_GamePfade.Text + "\\effects\\scripts\\StatusEffect.gd");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("export"))
                {
                    s_settings set = new s_settings();
                    set.valueType = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - (line.IndexOf('(') + 1));
                    int varEnd = line.IndexOf("var") + 4;
                    set.settingName = line.Substring(varEnd, (line.IndexOf(' ', varEnd) > 0 ? line.IndexOf(' ', varEnd) : line.Length) - varEnd);
                    _effect.effectSettings.Add(set);
                }
            }

            file.Close();

            try
            {
                file = new System.IO.StreamReader(textBox_GamePfade.Text + "\\effects\\scripts\\" + _effect.effectName.Substring(0, _effect.effectName.Length - 4) + "gd");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("export"))
                    {
                        s_settings set = new s_settings();
                        set.valueType = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - (line.IndexOf('(') + 1));
                        int varEnd = line.IndexOf("var") + 4;
                        set.settingName = line.Substring(varEnd, (line.IndexOf(' ', varEnd) > 0 ? line.IndexOf(' ', varEnd) : line.Length) - varEnd);
                        _effect.effectSettings.Add(set);
                    }
                }

                file.Close();
            }
            catch
            {

            }


            return _effect;
        }

        private void listBox_EffectsSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_EffectsSettings.SelectedIndex >= 0 && listBox_EffectsSettings.SelectedIndex < instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings.Count)
            {
                textBox_EffectsSettingsValueType.Text = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings[listBox_EffectsSettings.SelectedIndex].valueType;
                textBox_EffectsSettingsValue.Text = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings[listBox_EffectsSettings.SelectedIndex].value;
                button_SaveEffectsSettings.Enabled = true;
            }
        }

        private void button_SaveEffectsSettings_Click(object sender, EventArgs e)
        {
            if (listBox_EffectsSettings.SelectedIndex == -1) return;
            s_settings set = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings[listBox_EffectsSettings.SelectedIndex];
            instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings.Remove(instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings[listBox_EffectsSettings.SelectedIndex]);
            listBox_EffectsSettings.Items.RemoveAt(listBox_EffectsSettings.SelectedIndex);
            set.value = textBox_EffectsSettingsValue.Text;
            textBox_EffectsSettingsValue.Text = "";
            instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].effectSettings.Add(set);
            listBox_EffectsSettings.Items.Add(set.settingName + (set.value != "" && set.value != null ? " (" + set.value + ")" : ""));
        }

        private void button_DeleteEffect_Click(object sender, EventArgs e)
        {
            if (listBox_Effects.SelectedItems.Count > 0)
            {
                int index = listBox_Effects.SelectedIndex;
                instances[listBox_Enemys.SelectedIndex].effects.RemoveAt(index);
                listBox_Effects.Items.Remove(listBox_Effects.SelectedItem);
                textBox_EffectsSettingsValueType.Text = "";
                textBox_EffectsSettingsValue.Text = "";
                button_SaveEffectsSettings.Enabled = false;
            }
        }

        private void listBox_EffectsCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_AddEffectsCondition.Enabled = true;
        }

        private void button_AddEffectsCondition_Click(object sender, EventArgs e)
        {
            if (comboBox_EffectsCondtion.SelectedItem != null)
            {
                s_condition condition = new s_condition();
                condition.conditionName = comboBox_EffectsCondtion.SelectedItem.ToString();
                condition.value = textBox_EffectsCondtionsValue.Text;
                condition.conditionIndex = (comboBox_EffectsCondtion.SelectedIndex).ToString();
                listBox_EffectsCondition.Items.Add(condition.conditionName + (condition.value != "" && condition.value != null ? " (" + condition.value + ")" : ""));
                instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions.Add(condition);
            }
        }

        private void button_DeleteEffectsCondition_Click(object sender, EventArgs e)
        {
            if (listBox_EffectsCondition.SelectedItems.Count > 0)
            {
                int index = listBox_EffectsCondition.SelectedIndex;
                instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions.RemoveAt(index);
                listBox_EffectsCondition.Items.Remove(listBox_EffectsCondition.SelectedItem);
            }
        }

        private void button_WavePfadeFinder_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Wave Files|*.json";
            openFileDialog1.Title = "Select a Cursor File";

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .CUR file was selected, open it.  
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_WavePfade.Text = openFileDialog1.FileName;
            }
        }

        private void button_SaveWave_Click(object sender, EventArgs e)
        {
            if (textBox_WavePfade.Text == "")
            {
                button_WavePfadeFinder_Click(sender, e);
                if (textBox_WavePfade.Text == "") return;
            }

            List<string> lines = new List<string>();
            lines.Add("[");
            foreach (var inc in instances)
            {
                string enemyName = "\"" + "res:" + "\\Enemy\\Enemys\\" + inc.enemy + "\"";
                string enemySettings = "[";

                foreach (var setting in inc.enemySettings)
                {
                    if (setting.value != "" && setting.value != null) enemySettings += "[\"" + setting.settingName + "\"," + setting.value + "],";
                }
                enemySettings = enemySettings == "[" ? "[[null, null]]" : enemySettings + "]";

                string enemyEffects = "[";

                foreach (var effect in inc.effects)
                {
                    enemyEffects += "[\"" + "res:" + "\\effects\\" + effect.effectName + "\"";
                    string enemyEffectSetting = "[";
                    foreach (var setting in effect.effectSettings)
                    {
                        if (setting.value != "" && setting.value != null) enemyEffectSetting += "[\"" + setting.settingName + "\"," + setting.value + ",null],";
                    }
                    foreach (var condition in effect.conditions)
                    {
                        if (condition.value != "") enemyEffectSetting += "[" + "\"condition\"," + condition.conditionIndex + "," + condition.value + "],";
                    }
                    enemyEffectSetting = enemyEffectSetting == "[" ? "[[null,null,null]]" : enemyEffectSetting + "]";
                    enemyEffects += "," + enemyEffectSetting + "],";

                }
                enemyEffects = enemyEffects == "[" ? "[[null,[[null,null,null]]]]" : enemyEffects + "]";

                string pos = (inc.pos != null && inc.pos[0] + inc.pos[0] > 0) ? "[" + inc.pos[0].ToString() + "," + inc.pos[1].ToString() + "]" : "null";
                string delay = (inc.delay != null && inc.delay > 0) ? inc.delay.ToString() : "0";

                string line = "[[" + enemyName + "," + enemySettings + "]," + enemyEffects + "," + delay + "," + pos + "],";
                line = line.Replace("\\", "//");
                lines.Add(line);
            }
            lines.Add("]");


            System.IO.File.WriteAllLines(textBox_WavePfade.Text, lines);
        }

        private void numericUpDown_Delay_ValueChanged(object sender, EventArgs e)
        {
            s_instance inc = instances[listBox_Enemys.SelectedIndex];
            int index = listBox_Enemys.SelectedIndex;
            instances.RemoveAt(index);
            inc.delay = numericUpDown_Delay.Value;
            instances.Insert(index, inc);
        }

        private void numericUpDown_POSx_ValueChanged(object sender, EventArgs e)
        {
            s_instance inc = instances[listBox_Enemys.SelectedIndex];
            int index = listBox_Enemys.SelectedIndex;
            instances.RemoveAt(index);
            inc.pos = new List<int>();
            inc.pos.Add((int)numericUpDown_POSx.Value);
            inc.pos.Add((int)numericUpDown_POSy.Value);
            instances.Insert(index, inc);
        }

        private void numericUpDown_POSy_ValueChanged(object sender, EventArgs e)
        {
            s_instance inc = instances[listBox_Enemys.SelectedIndex];
            int index = listBox_Enemys.SelectedIndex;
            instances.RemoveAt(index);
            inc.pos = new List<int>();
            inc.pos.Add((int)numericUpDown_POSx.Value);
            inc.pos.Add((int)numericUpDown_POSy.Value);
            instances.Insert(index, inc);
        }

        private void button_LoadWave_Click(object sender, EventArgs e)
        {
            if (textBox_WavePfade.Text == "")
            {
                button_WavePfadeFinder_Click(sender, e);
                if (textBox_WavePfade.Text == "") return;
            }
            string json = System.IO.File.ReadAllText(textBox_WavePfade.Text);

            // var deserializedProduct = JsonConvert.DeserializeObject<List<List<string>>>(json);


        }
    }
}
