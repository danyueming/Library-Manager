﻿using Caliburn.Micro;
using ClientApplication.DBService;
using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientApplication.ViewModels
{
    public class LendingDetailsViewModel : BaseViewModel
    {
        public LendingDetailsViewModel(INavigationService navigationService, IWindowManager windowManager, IDBServiceManager<DatabaseServiceClient> dbServiceManager)
            : base(navigationService, windowManager, dbServiceManager)
        {
        }

        #region navigation properties

        public bool IsEditing { get; set; }

        #endregion

        #region bindable properties

        #region Lending

        private LendingDTO _lending;

        public LendingDTO Lending
        {
            get
            {
                return _lending;
            }
            set
            {
                if (_lending != value)
                {
                    _lending = value;
                    NotifyOfPropertyChange(() => Lending);
                }
            }
        }
        #endregion

        #region SelectedBook

        private LentBookDTO _selectedBook;

        public LentBookDTO SelectedBook
        {
            get
            {
                return _selectedBook;
            }
            set
            {
                if (_selectedBook != value)
                {
                    _selectedBook = value;
                    NotifyOfPropertyChange(() => SelectedBook);
                }
            }
        }
        #endregion

        #endregion

        #region operations

        public void Save()
        {
            if (Lending.Books.Count == 0)
            {
                MessageBox.Show("Dodaj książki, aby móc zapisać wypożyczenie. Jeśli chcesz je usunąć całkowicie, zrób to z głównego okna programu.",
                                "Uwaga", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var lentBook in Lending.Books)
                lentBook.Book = null;

            if (IsEditing)
            {
                using (var dbService = _dbServiceManager.GetService())
                {
                    dbService.EditLending(Lending);
                }                
            }
            else
            {
                using (var dbService = _dbServiceManager.GetService())
                {
                    dbService.AddLending(Lending);
                }   
            }

            TryClose(true);
        }

        public void DeleteBook()
        {
            var tmp = SelectedBook;
            SelectedBook = null;
            Lending.Books.Remove(tmp);
        }

        public void AddBook()
        {
            _navigationService.GetWindow<BooksListViewModel>()
                .WithParam(vm => vm.BookSelectionMode, true)
                .DoBeforeShow(vm => vm.BookSelectEvent += LendingDetailsViewModel_BookSelectedEvent)
                .ShowWindowModal();
        }

        void LendingDetailsViewModel_BookSelectedEvent(BookSelectedEventArgs<BookDTO> e)
        {
            if (Lending.Books.Any(lb => lb.BookId == e.Book.Id))
            {
                MessageBox.Show(String.Format("Książka \"{0}\" została już dodana do tego wypożyczenia.", e.Book.Title), "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Lending.Books.Add(new LentBookDTO() 
            { 
                Book = e.Book, 
                BookId = e.Book.Id, 
                LendingId = this.Lending.Id, 
                EndDate = this.Lending.EndDate 
            });
        }

        #endregion
    }
}