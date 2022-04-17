﻿using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class EditorCategoryViewModel
    {
        [Required(ErrorMessage = "O campo name é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo deve ter entre 3 e 40 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O campo slug é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo deve ter entre 3 e 40 caracteres")]
        public string Slug { get; set; }
    }
}