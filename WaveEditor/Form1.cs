﻿using System;
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
using Newtonsoft.Json.Linq;

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
        public List<s_settings> conditionSettings;
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
        public class DictionaryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { this.WriteValue(writer, value); }

            private void WriteValue(JsonWriter writer, object value)
            {
                var t = JToken.FromObject(value);
                switch (t.Type)
                {
                    case JTokenType.Object:
                        this.WriteObject(writer, value);
                        break;
                    case JTokenType.Array:
                        this.WriteArray(writer, value);
                        break;
                    default:
                        writer.WriteValue(value);
                        break;
                }
            }

            private void WriteObject(JsonWriter writer, object value)
            {
                writer.WriteStartObject();
                var obj = value as IDictionary<string, object>;
                foreach (var kvp in obj)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteValue(writer, kvp.Value);
                }
                writer.WriteEndObject();
            }

            private void WriteArray(JsonWriter writer, object value)
            {
                writer.WriteStartArray();
                var array = value as IEnumerable<object>;
                foreach (var o in array)
                {
                    this.WriteValue(writer, o);
                }
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return ReadValue(reader);
            }

            private object ReadValue(JsonReader reader)
            {
                while (reader.TokenType == JsonToken.Comment)
                {
                    if (!reader.Read()) throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }

                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        return ReadObject(reader);
                    case JsonToken.StartArray:
                        return this.ReadArray(reader);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Undefined:
                    case JsonToken.Null:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return reader.Value;
                    default:
                        throw new JsonSerializationException
                            (string.Format("Unexpected token when converting IDictionary<string, object>: {0}", reader.TokenType));
                }
            }

            private object ReadArray(JsonReader reader)
            {
                IList<object> list = new List<object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Comment:
                            break;
                        default:
                            var v = ReadValue(reader);

                            list.Add(v);
                            break;
                        case JsonToken.EndArray:
                            return list;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            private object ReadObject(JsonReader reader)
            {
                var obj = new Dictionary<string, object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            var propertyName = reader.Value.ToString();

                            if (!reader.Read())
                            {
                                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                            }

                            var v = ReadValue(reader);

                            obj[propertyName] = v;
                            break;
                        case JsonToken.Comment:
                            break;
                        case JsonToken.EndObject:
                            return obj;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            public override bool CanConvert(Type objectType) { return typeof(IDictionary<string, object>).IsAssignableFrom(objectType); }
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


            ParentDirectory = new System.IO.DirectoryInfo(textBox_GamePfade.Text + "\\effects\\conditions");

            foreach (System.IO.FileInfo f in ParentDirectory.GetFiles())
            {
                if (f.Name != "Condition.tscn" && f.Name != "Conditions.tscn")
                {
                    comboBox_EffectsCondtion.Items.Add(f.Name);
                }
            }
            comboBox_EffectsCondtion.SelectedIndex = 0;

            button_LoadWave.Enabled = true;
            button_SaveWave.Enabled = true; 
            button_Save_as.Enabled = true;
        }


        private void button_AddEnemy_Click(object sender, EventArgs e)
        {
            if (comboBox_Enemys.SelectedItem != null)
            {
                AddEnemy(comboBox_Enemys.SelectedItem.ToString());
            }


        }
        private void AddEnemy(string enemyName)
        {
            s_instance inc = new s_instance();
            inc.enemy = enemyName;
            listBox_Enemys.Items.Add(inc.enemy);
            inc.effects = new List<s_effect>();
            inc = addSettingsToInstance(inc);
            instances.Add(inc);
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
            set.value = textBox_EnemysSettingsValue.Text.Replace(",", ".");
            textBox_EnemysSettingsValue.Text = "";
            instances[listBox_Enemys.SelectedIndex].enemySettings.Add(set);
            listBox_EnemysSettings.Items.Add(set.settingName + (set.value != "" && set.value != null ? " (" + set.value + ")" : ""));
        }

        private void button_AddEffect_Click(object sender, EventArgs e)
        {
            if (comboBox_Effects.SelectedItem != null)
            {
                AddEffect(comboBox_Effects.SelectedItem.ToString());
            }
        }
        private void AddEffect(string effectName)
        {
            s_effect effect = new s_effect();
            effect.effectName = effectName; 
            listBox_Effects.Items.Add(effect.effectName);
            effect = addSettingsToEffect(effect);
            effect.conditions = new List<s_condition>();
            instances[listBox_Enemys.SelectedIndex].effects.Add(effect);
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
            }

            listBox_EffectsCondition.Items.Clear();
            listBox_ConditionsSettings.Items.Clear();
            textBox_EffectsCondtionsValue.Text = "";
            textBox_EffectsCondtionsValueType.Text = "";
            button_SaveConditionsSettings.Enabled = false;
            if (listBox_Effects.SelectedIndex >= 0 && listBox_Effects.SelectedIndex < instances[listBox_Enemys.SelectedIndex].effects.Count)
            {
                if (instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions != null)
                {
                    foreach (var condition in instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions)
                    {
                        listBox_EffectsCondition.Items.Add(condition.conditionName);
                    }
                }
                button_AddEffectsCondition.Enabled = true;
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

        private s_condition addSettingsToCondition(s_condition _condition)
        {
            string line;

            _condition.conditionSettings = new List<s_settings>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(textBox_GamePfade.Text + "\\effects\\conditions\\scripts\\Condition.gd");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("export"))
                {
                    s_settings set = new s_settings();
                    set.valueType = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - (line.IndexOf('(') + 1));
                    int varEnd = line.IndexOf("var") + 4;
                    set.settingName = line.Substring(varEnd, (line.IndexOf(' ', varEnd) > 0 ? line.IndexOf(' ', varEnd) : line.Length) - varEnd);
                    _condition.conditionSettings.Add(set);
                }
            }

            file.Close();

            try
            {
                file = new System.IO.StreamReader(textBox_GamePfade.Text + "\\effects\\conditions\\scripts\\" + _condition.conditionName.Substring(0, _condition.conditionName.Length - 4) + "gd");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("export"))
                    {
                        s_settings set = new s_settings();
                        set.valueType = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - (line.IndexOf('(') + 1));
                        int varEnd = line.IndexOf("var") + 4;
                        set.settingName = line.Substring(varEnd, (line.IndexOf(' ', varEnd) > 0 ? line.IndexOf(' ', varEnd) : line.Length) - varEnd);
                        _condition.conditionSettings.Add(set);
                    }
                }

                file.Close();
            }
            catch
            {

            }


            return _condition;
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
            set.value = textBox_EffectsSettingsValue.Text.Replace(",", ".");
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
            listBox_ConditionsSettings.Items.Clear();
            if (listBox_EffectsCondition.SelectedIndex >= 0 && listBox_EffectsCondition.SelectedIndex < instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions.Count)
            {
                if (instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings != null)
                {
                    foreach (var setting in instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings)
                    {
                        listBox_ConditionsSettings.Items.Add(setting.settingName + (setting.value != "" && setting.value != null ? " (" + setting.value + ")" : ""));
                    }
                }
            }
            textBox_EffectsCondtionsValue.Text = "";
            textBox_EffectsCondtionsValueType.Text = "";
            button_SaveConditionsSettings.Enabled = false;
        }

        private void button_AddEffectsCondition_Click(object sender, EventArgs e)
        {
            if (comboBox_EffectsCondtion.SelectedItem != null)
            {
                AddEffectsCondition(comboBox_EffectsCondtion.SelectedItem.ToString());
            }
        }

        private void AddEffectsCondition(string name)
        {
            s_condition condition = new s_condition();
            condition.conditionName = name;
            listBox_EffectsCondition.Items.Add(condition.conditionName);
            condition = addSettingsToCondition(condition);
            instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions.Add(condition);
        }

        private void button_DeleteEffectsCondition_Click(object sender, EventArgs e)
        {
            if (listBox_EffectsCondition.SelectedItems.Count > 0)
            {
                int index = listBox_EffectsCondition.SelectedIndex;
                instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions.RemoveAt(index);
                listBox_EffectsCondition.Items.Remove(listBox_EffectsCondition.SelectedItem);
                textBox_EffectsCondtionsValueType.Text = "";
                textBox_EffectsCondtionsValue.Text = "";
                button_SaveConditionsSettings.Enabled = false;
            }
        }

        private void button_WavePfadeFinder_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Filter = "Wave Files|*.json";
            FileDialog.Title = "Select a Cursor File";

            FileDialog.FileName = textBox_EggName.Text + ".json";

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .CUR file was selected, open it.  
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_WavePfade.Text = FileDialog.FileName;
            }
        }
        private void button_save_WavePfadeFinder_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            SaveFileDialog FileDialog = new SaveFileDialog();
            FileDialog.Filter = "Wave Files|*.json";
            FileDialog.Title = "Select a Cursor File";

            FileDialog.FileName = textBox_EggName.Text + ".json";

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .CUR file was selected, open it.  
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_WavePfade.Text = FileDialog.FileName;
            }
        }
        private void button_SaveWave_Click(object sender, EventArgs e)
        {
            if (textBox_WavePfade.Text == "")
            {
                button_save_WavePfadeFinder_Click(sender, e);
                if (textBox_WavePfade.Text == "") return;
            }

            List<string> lines = new List<string>();
            lines.Add("{");

            string eggName = "\"eggName\" : \" " + textBox_EggName.Text + "\" ,";
            lines.Add(eggName);

            string tier = "\"tier\" : " +  numericUpDown_Tier.Value + " ,";
            lines.Add(tier);

            string small_boss = "\"small_boss\" : " + (checkBox_small_boss.Checked ? "true" : "false") + " ,";
            lines.Add(small_boss);

            string boss = "\"boss\" : " + (checkBox_Boss.Checked ? "true" : "false") + " ,";
            lines.Add(boss);

            lines.Add("\"enemeys\" : [");

            foreach (var inc in instances)
            {
                lines.Add("{");

                string packed_scene = "\"packed_scene\" : " + "\"res:\\\\Enemy\\Enemys\\" + inc.enemy + "\" ,";
                packed_scene = packed_scene.Replace("\\", "/");
                lines.Add(packed_scene);

                lines.Add("\"not_default_values\" : ");
                lines.Add("{");
                foreach (var setting in inc.enemySettings)
                {
                    string enemySettings = "";
                    if (setting.value != "" && setting.value != null)
                    {
                        enemySettings += "\"" + setting.settingName + "\" :" + setting.value + ",";
                        lines.Add(enemySettings);
                    }
                }
                lines.Add("},");


                lines.Add("\"effects\" : ");
                lines.Add("[");


                foreach (var effect in inc.effects)
                {
                    lines.Add("{");
                    string effect_packed_scene = "\"packed_scene\" : " + "\"res:\\\\Effects\\" + effect.effectName + "\" ,";
                    effect_packed_scene = effect_packed_scene.Replace("\\", "/");
                    lines.Add(effect_packed_scene);

                    lines.Add("\"not_default_values\" : ");
                    lines.Add("{");

                    foreach (var setting in effect.effectSettings)
                    {
                        string enemyEffectSetting = "";
                        if (setting.value != "" && setting.value != null)
                        {
                            enemyEffectSetting += "\"" + setting.settingName + "\" :" + setting.value + ",";
                            lines.Add(enemyEffectSetting);
                        }
                    }
                    lines.Add("},");

                    lines.Add("\"conditions\" : ");
                    lines.Add("[");
                    foreach (var condition in effect.conditions)
                    {
                        lines.Add("{");
                        string conditio_packed_scene = "\"packed_scene\" : " + "\"res:\\\\Effects\\conditions\\" + condition.conditionName + "\" ,";
                        conditio_packed_scene = conditio_packed_scene.Replace("\\", "/");
                        lines.Add(conditio_packed_scene);

                        lines.Add("\"not_default_values\" : ");
                        lines.Add("{");

                        foreach (var setting in condition.conditionSettings)
                        {
                            string conditionSetting = "";
                            if (setting.value != "" && setting.value != null)
                            {
                                conditionSetting += "\"" + setting.settingName + "\" :" + setting.value + ",";
                                lines.Add(conditionSetting);
                            }
                        }
                        lines.Add("}");
                        lines.Add("},");
                    }
                    lines.Add("],");

                    lines.Add("},");
                }
                lines.Add("],");


                lines.Add("},");
            }
            lines.Add("]");

            lines.Add("}");
            System.IO.File.WriteAllLines(textBox_WavePfade.Text, lines);

        }



        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
		{

		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
            if(checkBox_Boss.Checked) checkBox_Boss.Checked = false;
        }

		private void checkBox_Boss_CheckedChanged(object sender, EventArgs e)
		{
            if (checkBox_small_boss.Checked) checkBox_small_boss.Checked = false;
        }

        private void tableLayoutPanel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listBox_ConditionsSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_ConditionsSettings.SelectedIndex >= 0 && listBox_ConditionsSettings.SelectedIndex < instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings.Count)
            {
                textBox_EffectsCondtionsValueType.Text = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings[listBox_ConditionsSettings.SelectedIndex].valueType;
                textBox_EffectsCondtionsValue.Text      = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings[listBox_ConditionsSettings.SelectedIndex].value;
                button_SaveConditionsSettings.Enabled = true;
            }
        }

        private void button_SaveConditionsSettings_Click(object sender, EventArgs e)
        {
            if (listBox_ConditionsSettings.SelectedIndex == -1) return;
            s_settings set = instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings[listBox_ConditionsSettings.SelectedIndex];
            instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings.Remove(instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings[listBox_ConditionsSettings.SelectedIndex]);
            listBox_ConditionsSettings.Items.RemoveAt(listBox_ConditionsSettings.SelectedIndex);
            set.value = textBox_EffectsCondtionsValue.Text.Replace(",", ".");
            textBox_EffectsCondtionsValue.Text = "";
            instances[listBox_Enemys.SelectedIndex].effects[listBox_Effects.SelectedIndex].conditions[listBox_EffectsCondition.SelectedIndex].conditionSettings.Add(set);
            listBox_ConditionsSettings.Items.Add(set.settingName + (set.value != "" && set.value != null ? " (" + set.value + ")" : ""));
        }


        private void button_LoadWave_Click(object sender, EventArgs e)
        {
            if (textBox_WavePfade.Text == "")
            {
                button_WavePfadeFinder_Click(sender, e);
                if (textBox_WavePfade.Text == "") return;
            }
            string json = System.IO.File.ReadAllText(textBox_WavePfade.Text);

            instances.Clear();
            listBox_Enemys.Items.Clear();

            IDictionary<string, object> json_Dic = JsonConvert.DeserializeObject<IDictionary<string, object>>(json, new DictionaryConverter());

            object value = "";
            json_Dic.TryGetValue("eggName", out value);
            textBox_EggName.Text = (string)(value);

            json_Dic.TryGetValue("small_boss", out value);
            checkBox_small_boss.Checked = (bool)(value);

            json_Dic.TryGetValue("boss", out value);
            checkBox_Boss.Checked = (bool)(value);

            json_Dic.TryGetValue("tier", out value);
            numericUpDown_Tier.Value = (long)(value);

            json_Dic.TryGetValue("enemeys", out value);

            int i = 0;
            foreach (IDictionary<string, object> enemy in (List<object>)value)
            {
                object value_enemy = "";
                string temp_names = "";
                int temp_counter = 0;
                enemy.TryGetValue("packed_scene", out value);
                temp_names = (string)(value);
                temp_counter = temp_names.LastIndexOf('/') +1;
                temp_names = temp_names.Substring(temp_counter, temp_names.Length - temp_counter);

                AddEnemy(temp_names);
                listBox_Enemys.SelectedIndex = i;
                listBox_Enemys_SelectedIndexChanged(null, null);


                object value_enemy_settings = "";
                enemy.TryGetValue("not_default_values", out value_enemy_settings);

                foreach (var setting in (IDictionary<string, object>)value_enemy_settings)
                {
                    listBox_EnemysSettings.SelectedIndex = listBox_EnemysSettings.Items.IndexOf(setting.Key);
                    textBox_EnemysSettingsValue.Text = ((object)setting.Value).ToString();
                    button_SaveEnemysSettings_Click(null, null);
                }

                object value_enemy_effects = "";
                enemy.TryGetValue("effects", out value_enemy_effects);

                int n = 0;
                foreach (IDictionary<string, object> effect in (List<object>)value_enemy_effects)
                {
                    effect.TryGetValue("packed_scene", out value);
                    temp_names = (string)(value);
                    temp_counter = temp_names.LastIndexOf('/') + 1;
                    temp_names = temp_names.Substring(temp_counter, temp_names.Length - temp_counter);
                    AddEffect(temp_names);
                    listBox_Effects.SelectedIndex = n;
                    listBox_Effects_SelectedIndexChanged(null, null);

                    object value_enemy_effects_settings = "";
                    effect.TryGetValue("not_default_values", out value_enemy_effects_settings);
                    foreach (var effect_setting in (IDictionary<string, object>)value_enemy_effects_settings)
                    {
                        listBox_EffectsSettings.SelectedIndex = listBox_EffectsSettings.Items.IndexOf(effect_setting.Key);
                        textBox_EffectsSettingsValue.Text = ((object)effect_setting.Value).ToString();
                        button_SaveEffectsSettings_Click(null, null);
                    }


                    object value_enemy_effects_conditions = "";
                    effect.TryGetValue("conditions", out value_enemy_effects_conditions);

                    int x = 0;
                    foreach (IDictionary<string, object> condition in (List<object>)value_enemy_effects_conditions)
                    {
                        condition.TryGetValue("packed_scene", out value);
                        temp_names = (string)(value);
                        temp_counter = temp_names.LastIndexOf('/') + 1;
                        temp_names = temp_names.Substring(temp_counter, temp_names.Length - temp_counter);
                        AddEffectsCondition(temp_names);
                        listBox_EffectsCondition.SelectedIndex = x;
                        listBox_EffectsCondition_SelectedIndexChanged(null, null);

                        object value_enemy_effects_condition_settings = "";
                        condition.TryGetValue("not_default_values", out value_enemy_effects_condition_settings);
                        foreach (var condition_setting in (IDictionary<string, object>)value_enemy_effects_condition_settings)
                        {
                            listBox_ConditionsSettings.SelectedIndex = listBox_ConditionsSettings.Items.IndexOf(condition_setting.Key);
                            textBox_EffectsCondtionsValue.Text = ((object)condition_setting.Value).ToString();
                            button_SaveConditionsSettings_Click(null, null);
                        }

                        x++;
                    }

                    n++;
                }


                //instances.Add(temp_enemy);
                ++i;
            }
        }

        private void button_Save_as_Click(object sender, EventArgs e)
        {
            if (textBox_WavePfade.Text != "")
                button_save_WavePfadeFinder_Click(sender, e);
            button_SaveWave_Click(null, null);
        }
    }

    
}
