﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace 표준국어대사전.Classes
{
    public class HistoryManager
    {
        const int RECORD_LIMIT = 30;

        public bool CanGoBack { get { return (BackStack.Count > 0); } }
        public bool CanGoForward { get { return (ForwardStack.Count > 0); } }

        //이전으로 되돌리기 위한 스택
        LinkedList<RecordPoint> BackStack;

        //앞으로 되돌리기 위한 스택
        LinkedList<RecordPoint> ForwardStack;


        public HistoryManager()
        {
            this.BackStack = new LinkedList<RecordPoint>();
            this.ForwardStack = new LinkedList<RecordPoint>();
        }


        /// <summary>
        /// 검색 결과와 뜻풀이가 모두 변화될 때 이전 값을 기록
        /// </summary>
        public void RecordAll(string searchText, ObservableCollection<SearchResultItem> searchResults, WordDetailItem definition, int selectedIndex = -1, bool isRedo = false)
        {
            RecordPoint record = new RecordPoint(RecordType.ClearAll, searchText, searchResults, definition, selectedIndex);
            BackStack.AddLast(record);
            if (BackStack.Count > RECORD_LIMIT)
                BackStack.RemoveFirst();

            if (!isRedo && ForwardStack.Count != 0)
            {
                ForwardStack.Clear();
            }
        }

        /// <summary>
        /// 뜻풀이가 변화될 때 이전 값을 기록
        /// </summary>
        public void RecordDefinition(string searchText, WordDetailItem definition, int selectedIndex = -1, bool isRedo = false)
        {
            RecordPoint record = new RecordPoint(RecordType.ClearDefinition, searchText, null, definition, selectedIndex);
            BackStack.AddLast(record);
            if (BackStack.Count > RECORD_LIMIT)
                BackStack.RemoveFirst();

            if (!isRedo && ForwardStack.Count != 0)
            {
                ForwardStack.Clear();
            }
        }

        /// <summary>
        /// 되돌리기
        /// </summary>
        public void Undo(ref string searchText, ref ObservableCollection<SearchResultItem> searchResults, ref WordDetailItem definition, ref int selectedIndex)
        {
            if (BackStack.Count == 0)
                return;

            RecordPoint previousRecord = BackStack.Last();
            BackStack.RemoveLast();

            if (previousRecord.Type == RecordType.ClearDefinition)
            {
                RecordUndoDefinition(searchText, definition, selectedIndex);

                searchText = previousRecord.SearchText;
                selectedIndex = previousRecord.SelectedIndex;
                if (previousRecord.Definition != null)
                {
                    definition = previousRecord.Definition;
                }
            }
            else if (previousRecord.Type == RecordType.ClearAll)
            {
                RecordUndoAll(searchText, searchResults, definition, selectedIndex);

                searchText = previousRecord.SearchText;
                selectedIndex = previousRecord.SelectedIndex;
                if (previousRecord.SearchResults != null)
                {
                    searchResults.Clear();
                    for (int i = 0; i < previousRecord.SearchResults.Count; i++)
                    {
                        searchResults.Add(previousRecord.SearchResults[i]);
                    }
                }
                if (previousRecord.Definition != null)
                {
                    definition = previousRecord.Definition;
                }
            }
            else if (previousRecord.Type == RecordType.ClearSearchResult)
            {
                //일어나지 않는 케이스
            }
        }

        /// <summary>
        /// 다시 실행
        /// </summary>
        public void Redo(ref string searchText, ref ObservableCollection<SearchResultItem> searchResults, ref WordDetailItem definition, ref int selectedIndex)
        {
            if (ForwardStack.Count == 0)
                return;

            RecordPoint postRecord = ForwardStack.Last();
            ForwardStack.RemoveLast();

            if (postRecord.Type == RecordType.ClearDefinition)
            {
                RecordDefinition(searchText, definition, selectedIndex, true);

                searchText = postRecord.SearchText;
                selectedIndex = postRecord.SelectedIndex;
                if (postRecord.Definition != null)
                {
                    definition = postRecord.Definition;
                }
            }
            else if (postRecord.Type == RecordType.ClearAll)
            {
                RecordAll(searchText, searchResults, definition, selectedIndex, true);

                searchText = postRecord.SearchText;
                selectedIndex = postRecord.SelectedIndex;
                if (postRecord.SearchResults != null)
                {
                    searchResults.Clear();
                    for (int i = 0; i < postRecord.SearchResults.Count; i++)
                    {
                        searchResults.Add(postRecord.SearchResults[i]);
                    }
                }
                if (postRecord.Definition != null)
                {
                    definition = postRecord.Definition;
                }
            }
            else if (postRecord.Type == RecordType.ClearSearchResult)
            {
                //일어나지 않는 케이스
            }
        }


        /// <summary>
        /// 되돌리기로 검색 결과와 뜻풀이가 모두 변화될 때 다시 실행을 위해 현재 값을 기록(되돌리기 이전 실행)
        /// </summary>
        void RecordUndoAll(string searchText, ObservableCollection<SearchResultItem> searchResults, WordDetailItem definition, int selectedIndex = -1)
        {
            RecordPoint record = new RecordPoint(RecordType.ClearAll, searchText, searchResults, definition, selectedIndex);
            ForwardStack.AddLast(record);
            if (ForwardStack.Count > RECORD_LIMIT)
                ForwardStack.RemoveFirst();
        }

        /// <summary>
        /// 되돌리기로 뜻풀이가 변화될 때 다시 실행을 위해 현재 값을 기록(되돌리기 이전 실행)
        /// </summary>
        void RecordUndoDefinition(string searchText, WordDetailItem definition, int selectedIndex = -1)
        {
            RecordPoint record = new RecordPoint(RecordType.ClearDefinition, searchText, null, definition, selectedIndex);
            ForwardStack.AddLast(record);
            if (ForwardStack.Count > RECORD_LIMIT)
                ForwardStack.RemoveFirst();
        }



    }
}
