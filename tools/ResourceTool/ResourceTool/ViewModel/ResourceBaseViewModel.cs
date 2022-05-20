﻿using ResourceTool.Model;
using ResourceTool.Service;
using ResourceTool.Utils;
using ResourceTool.ViewModel.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ResourceTool.ViewModel
{
    public class ResourceBaseViewModel : BaseViewModel
    {
        private string _rootPath;
        public string RootPath
        {
            get { return _rootPath; }
            set
            {
                if (value == _rootPath)
                {
                    return;
                }

                _rootPath = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ResourceViewModel> Resources { get; private init; }

        private ResourceViewModel _selectedResource;
        public ResourceViewModel SelectedResource
        {
            get { return _selectedResource; }
            set
            {
                if (value == _selectedResource)
                {
                    return;
                }

                _selectedResource = value;
                OnPropertyChanged();
            }
        }

        private string TextureDirectory { get { return Path.Combine(RootPath, "Textures"); } }

        public ICommand CreateTextureCommand { get; private init; }
        public ICommand CreateBlockCommand { get; private init; }


        public ResourceBaseViewModel(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentNullException(nameof(rootPath));
            }

            _rootPath = rootPath;
            _name = string.Empty;
            Resources = new ObservableCollection<ResourceViewModel>();

            CreateTextureCommand = new RelayCommand(CreateTextureCommandExecute);
            CreateBlockCommand = new RelayCommand(CreateBlockCommandExecute);
        }

        public ResourceBaseViewModel(string rootPath, string name) : this(rootPath)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public ResourceBaseViewModel(string rootPath, ResourceBase resourceBase) : this(rootPath)
        {
            Name = resourceBase.Name;

            var resources = 
                resourceBase.Textures.Select(t => new TextureViewModel(t)).Cast<ResourceViewModel>()
                .Concat(resourceBase.Blocks.Select(b => new BlockViewModel(b)).Cast<ResourceViewModel>());

            Resources = new ObservableCollection<ResourceViewModel>(resources);
        }


        public ResourceBase GetModel()
        {
            ResourceBase resourceBase = new ResourceBase(
                _name,
                Resources.OfType<TextureViewModel>().Select(t => t.GetModel()),
                Resources.OfType<BlockViewModel>().Select(t => t.GetModel())
                );

            return resourceBase;
        }


        private void CreateTextureCommandExecute(object parameter)
        {
            var dialogService = App.ServiceProvider.GetService(typeof(IDialogService)) as IDialogService;
            if (dialogService == null)
            {
                throw new NullReferenceException("Dialog service wasn't found");
            }

            CreateTextureDialogViewModel dialogVM = new CreateTextureDialogViewModel();
            bool? dialogResult = dialogService.ShowDialog(dialogVM);
            if (dialogResult.HasValue && dialogResult.Value)
            {
                string fileName = Path.GetFileName(dialogVM.Path);
                string targetPath = Path.Combine(TextureDirectory, fileName);
            
                if (!File.Exists(targetPath))
                    File.Copy(dialogVM.Path, targetPath);
            
                TextureViewModel textureVM = new TextureViewModel(Guid.NewGuid(), targetPath, dialogVM.Name);
                Resources.Add(textureVM);
            }
        }

        private void CreateBlockCommandExecute(object parameter)
        {
            var dialogService = App.ServiceProvider.GetService(typeof(IDialogService)) as IDialogService;
            if (dialogService == null)
            {
                throw new NullReferenceException("Dialog service wasn't found");
            }

            CreateBlockDialogViewModel dialogVM = new CreateBlockDialogViewModel();
            bool? dialogResult = dialogService.ShowDialog(dialogVM);
            if (dialogResult.HasValue && dialogResult.Value)
            {
                BlockViewModel blockVM = dialogVM.GetBlockViewModel();
                Resources.Add(blockVM);
            }
        }
    }
}