﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace 표준국어대사전.Classes
{
    public static class StorageManager
    {
        public const string FirstSetup = "#FirstSetup";                                 //int
        public const string DisplayFont = "#DisplayFont";                               //string
        public const string UseCustomAPIKey = "#UseCustomAPIKey";                       //bool
        public const string APIKey = "#APIKey";                                         //string
        public const string SpellingCheckerAgreement = "#SpellingCheckerAgreement";     //bool
        public const string Language = "#Language";                                     //string
        public const string ColorTheme = "#ColorTheme";                                 //string(Light, Dark, system)
        public const string MemoData = "#MemoData";                                     //string
        public const string RecentWord = "#RecentWord";                                 //string(연결리스트: ,로 단어 나열)
        public const string FONTMAGNIFICATION = "#FONTMAGNIFICATION";                   //double(1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6)

        //Lab Function
        public const string LabWordReaderEnabled = "#LabWordReaderEnabled";             //bool

        public static void StartUpSetup()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values[FirstSetup] == null)
                FirstUpSetUp();

            //2.0.0.0 업데이트시 불필요한 설정 제거
            if ((int)localSettings.Values[FirstSetup] < 2)
            {
                localSettings.Values.Remove("#DisplayFontSize");
                localSettings.Values.Remove("#FontCheckNoLater");
                localSettings.Values.Remove("#UseOriginWeb");
                localSettings.Values[FirstSetup] = 2;
            }
            //2.0.1.0 업데이트 - 언어
            if ((int)localSettings.Values[FirstSetup] < 3)
            {
                localSettings.Values[Language] = "system";
                localSettings.Values[FirstSetup] = 3;
            }

            //2.1.0.0 - 라이트/다크 모드 지원 // 실험실 기능
            if ((int)localSettings.Values[FirstSetup] < 4)
            {
                localSettings.Values[ColorTheme] = "system";
                localSettings.Values[FirstSetup] = 4;
                //LabFunction
                localSettings.Values[LabWordReaderEnabled] = false;
            }

            //2.2.2.2 - 메모 저장
            if ((int)localSettings.Values[FirstSetup] < 5)
            {
                localSettings.Values[FirstSetup] = 5;
                localSettings.Values[MemoData] = "";
            }

            //2.3.0.0 - 최근 검색 단어
            if ((int)localSettings.Values[FirstSetup] < 6)
            {
                localSettings.Values[FirstSetup] = 6;
                localSettings.Values[RecentWord] = "";
            }

            //2.4.0.0 - 웹뷰 단어 검색 기능 삭제
            if ((int)localSettings.Values[FirstSetup] < 7)
            {
                localSettings.Values[FirstSetup] = 7;
                localSettings.Values.Remove("#SearchEngine");
                localSettings.Values.Remove("#UseDevelopermode");
                localSettings.Values[FONTMAGNIFICATION] = 1.0;
            }
        }

        public static void FirstUpSetUp()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            //최초 설정
            localSettings.Values[FirstSetup] = 0;
            localSettings.Values[DisplayFont] = "나눔바른고딕 옛한글";
            localSettings.Values[UseCustomAPIKey] = false;
            localSettings.Values[APIKey] = "C58534E2D39CF7CA69BCA193541C1688";
            localSettings.Values[SpellingCheckerAgreement] = false;
            localSettings.Values[Language] = "system";
            localSettings.Values[ColorTheme] = "system";
            localSettings.Values[MemoData] = "";
            localSettings.Values[RecentWord] = "";
            localSettings.Values[FONTMAGNIFICATION] = 1.0;

            //LabFunction
            localSettings.Values[LabWordReaderEnabled] = false;
        }

        public static T GetSetting<T>(string name)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (T)localSettings.Values[name];
        }

        public static void SetSetting<T>(string name, T value)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[name] = value;
        }

        public static void Clear()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Clear();
        }
    }
}
