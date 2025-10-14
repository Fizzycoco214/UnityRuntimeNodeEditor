using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DetailsPanel : MonoBehaviour
{
    public static DetailsPanel Instance { private set; get; }


    public TMP_Text Header;
    public TMP_Text IDText;
    public VerticalLayoutGroup panel;

    private Node inspectedNode;

    public UnityEvent OnDetailPanelDraw = new UnityEvent();

    void Awake()
    {
        Instance = this;
    }


    public void ShowPanel(Node node)
    {
        //Avoid redrawing the same detail panel
        if (inspectedNode == node || node.headerText == null)
        {
            return;
        }


        ClearDetailPanel();
        inspectedNode = node;
        Header.text = node.headerText.text;
        IDText.text = "ID: " + node.ID;

        gameObject.SetActive(true);
        CreateDetailPanel(node);
        OnDetailPanelDraw.Invoke();
    }

    public void HidePanel()
    {
        inspectedNode = null;
        ClearDetailPanel();
        gameObject.SetActive(false);
    }

    void ClearDetailPanel()
    {
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void CreateDetailPanel(Node node)
    {
        FieldInfo[] details = node.GetType().GetFields();
        foreach (FieldInfo field in details)
        {
            foreach (object attribute in field.GetCustomAttributes(true))
            {
                if (attribute is Inspectable detail)
                {
                    AddDetail(detail, field);
                }
            }
        }

        node.ShowDetails(this);
    }


    void AddDetail(Inspectable detail, FieldInfo detailInfo)
    {
        switch (detailInfo.FieldType)
        {
            case Type t when t == typeof(string):
                string stringValue = (string)detailInfo.GetValue(inspectedNode);
                TextDetail stringDetail = LoadDetailPrefab<TextDetail>("Text Detail");

                stringDetail.name = detail.name;
                stringDetail.title.text = detail.name;
                stringDetail.inputField.SetTextWithoutNotify(stringValue);

                stringDetail.inputField.onEndEdit.AddListener(
                    (string value) => detailInfo.SetValue(inspectedNode, value)
                );
                break;
            case Type t when t == typeof(float):
                float floatValue = (float)detailInfo.GetValue(inspectedNode);
                TextDetail floatDetail = LoadDetailPrefab<TextDetail>("Text Detail");

                floatDetail.name = detail.name;
                floatDetail.title.text = detail.name;
                floatDetail.inputField.SetTextWithoutNotify(floatValue.ToString());

                floatDetail.inputField.onEndEdit.AddListener(
                    (string value) => detailInfo.SetValue(inspectedNode, value)
                );
                break;
            case Type t when t == typeof(bool):
                bool boolValue = (bool)detailInfo.GetValue(inspectedNode);
                BooleanDetail boolDetail = LoadDetailPrefab<BooleanDetail>("Boolean Detail");

                boolDetail.name = detail.name;
                boolDetail.title.text = detail.name;
                boolDetail.toggle.SetIsOnWithoutNotify(boolValue);

                boolDetail.toggle.onValueChanged.AddListener(
                    (bool value) => detailInfo.SetValue(inspectedNode, value)
                );

                break;
            case Type t when t == typeof(Toggle):
                Toggle toggle = detailInfo.GetValue(inspectedNode) as Toggle;
                BooleanDetail booleanDetail = LoadDetailPrefab<BooleanDetail>("Boolean Detail");

                booleanDetail.name = detail.name;
                booleanDetail.title.text = detail.name;
                booleanDetail.toggle.SetIsOnWithoutNotify(toggle.isOn);

                //Changing one will update the other
                booleanDetail.toggle.onValueChanged.AddListener(
                    (bool value) => toggle.isOn = value);

                toggle.onValueChanged.AddListener(
                    (bool value) => booleanDetail.toggle.SetIsOnWithoutNotify(value));
                break;
            case Type t when t == typeof(TMP_Dropdown):
                TMP_Dropdown dropdown = detailInfo.GetValue(inspectedNode) as TMP_Dropdown;
                DropdownDetail dropdownDetail = LoadDetailPrefab<DropdownDetail>("Dropdown Detail");

                dropdownDetail.name = detail.name;
                dropdownDetail.title.text = detail.name;

                dropdownDetail.dropdown.ClearOptions();
                List<TMP_Dropdown.OptionData> options = new();
                foreach (object option in Enum.GetValues(detail.modifiers[0] as Type))
                {
                    options.Add(new TMP_Dropdown.OptionData(option.ToString()));
                }

                dropdownDetail.dropdown.AddOptions(options);
                dropdownDetail.dropdown.SetValueWithoutNotify(dropdown.value);

                dropdownDetail.dropdown.onValueChanged.AddListener(
                    (int value) => dropdown.value = value
                );

                dropdown.onValueChanged.AddListener(
                    (int value) => dropdownDetail.dropdown.SetValueWithoutNotify(value)
                );

                break;
            default:
                Debug.Log("Nothing found");
                break;
        }
    }

    public T LoadDetailPrefab<T>(string path)
    {
        GameObject detailPrefab = Resources.Load<GameObject>("Details/" + path);
        return Instantiate(detailPrefab, panel.transform).GetComponent<T>();
    }
}
