import { NavigationService } from './../services/navigation.service';
import { Component, ElementRef, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Product, Category, Offer } from '../models/models';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { response } from 'express';

@Component({
  selector: 'app-add-product',
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.css']
})
export class AddProductComponent implements OnInit {
  product: Product = {
    id: 0,
    title: '',
    description: '',
    productCategory: {
      id: 0,
      category: '',
      subCategory: ''
    },
    offer: {
      id: 0,
      title: '',
      discount: 0
    },
    price: 0,
    quantity: 0,
    image: '',
  };

  categories: Category[] = [];
  offers: Offer[] = [];
  selectedImage: string | ArrayBuffer | null = null;
  productToEdit: Product | null = null;
  showAddForm: boolean = false;
  imageFile: File | null = null;
  selectedFile: File | undefined;
  editingProductId: number | null = null;
  formBuilder: any;
  showEditSection: boolean = false;
  constructor(private http: HttpClient, private router: Router, private NavigationService: NavigationService) { }

  addedProducts: Product[] = [];

  ngOnInit(): void {
    this.fetchCategories();
    this.fetchOffers();
    this.fetchProducts();
  }

  fetchCategories(): void {
    this.http.get<Category[]>('https://localhost:7149/api/Shopping/GetCategoryList')
      .subscribe(
        (response: Category[]) => {
          this.categories = response;
        },
        (error) => {
          console.error('Error fetching categories:', error);
        }
      );
  }

  fetchOffers(): void {
    this.http.get<Offer[]>('https://localhost:7149/api/Shopping/GetOffers') // Adjust endpoint as per your API
      .subscribe(
        (response: Offer[]) => {
          this.offers = response;
        },
        (error) => {
          console.error('Error fetching offers:', error);
        }
      );
  }

  fetchProducts(): void {
    this.http.get<Product[]>('https://localhost:7149/api/Shopping/GetAllProducts')
      .subscribe(
        (response: Product[]) => {
          this.addedProducts = response;
        },
        (error) => {
          console.error('Error fetching products:', error);
        },
        () => {


        }
      );
  }



  onImageSelected(event: any): void {
    this.imageFile = event.target.files[0];
  }

  saveImageLocally(imagePath: string): void {
  }




  deleteProduct(productId: number): void {
    const index = this.addedProducts.findIndex(product => product.id === productId);

    if (index !== -1) {

      this.http.delete(`https://localhost:7149/api/Shopping/DeleteProduct/${productId}`)
        .subscribe(
          () => {
            console.log('Product deleted successfully');
            this.addedProducts.splice(index, 1);
          },
          (error) => {
            console.error('Error deleting product:', error);
            alert('Error deleting product. Please try again.');
          }
        );
    }
  }

  toggleAddForm(): void {
    this.showAddForm = !this.showAddForm;
    if (!this.showAddForm) {
      this.resetForm();
    }
  }


  resetForm(): void {
    this.product = {
      id: 0,
      title: '',
      description: '',
      productCategory: {
        id: 0,
        category: '',
        subCategory: ''
      },
      offer: {
        id: 0,
        title: '',
        discount: 0
      },
      price: 0,
      quantity: 0,
      image: '',
    };
    this.imageFile = null;

  }




  cancelEdit(): void {
    this.productToEdit = null;
  }

  cancelAdd(): void {
    this.showAddForm = false;
    this.resetForm();
  }



  public productForm: FormGroup = new FormGroup({
    id: new FormControl(0),
    title: new FormControl("", [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern('^[a-zA-Z ]+$')]),
    price: new FormControl("", [Validators.required, Validators.min(1000), Validators.pattern('^[0-9]+$')]),
    quantity: new FormControl("", [Validators.required, Validators.min(1), Validators.pattern('^[0-9]+$')]),
    offer: new FormControl("", [Validators.required]),
    productCategory: new FormControl("", [Validators.required]),
    description: new FormControl("", [Validators.required, Validators.minLength(5), Validators.pattern('^[a-zA-Z ]+$')]),
    file: new FormControl(null, []),
  });

  scrollToEdit(editSection: HTMLElement) {
    // editSection.scrollIntoView({ behavior: 'smooth' });
    const yOffset = editSection.getBoundingClientRect().top + window.pageYOffset;
    window.scrollTo({ top: yOffset, behavior: 'smooth' });
  }


  fetchProductData(id: number) {
    this.editingProductId = id;
    this.showEditSection = true;
    this.NavigationService.getProductById(id).subscribe(
      (productData: Product) => {
        this.productForm.patchValue({
          id: productData.id,
          title: productData.title,
          productCategory: productData.productCategory ? productData.productCategory.id : null,
          offer: productData.offer ? productData.offer.id : null,
          price: productData.price,
          quantity: productData.quantity,
          description: productData.description,
          file: productData.image
        });
      },
      error => {
        console.error('Error fetching product data:', error);
      }
    );
  }


  submitForm(): void {
      if (this.editingProductId) {
        console.log(this.editingProductId);
        const formData = new FormData();
        console.log('Title value:', this.productForm.get('title')?.value);
        console.log('Description value:', this.productForm.get('description')?.value);
        console.log('Price value:', this.productForm.get('price')?.value);
        console.log('Quantity value:', this.productForm.get('quantity')?.value);
        console.log('Category value:', this.productForm.get('productCategory')?.value);
        console.log('Offer value:', this.productForm.get('offer')?.value);
        console.log('Image value:', this.productForm.get('file')?.value);

        const productFormValues = this.productForm.value;
        for (const key in productFormValues) {
          formData.append(key, productFormValues[key]);
        }
        formData.forEach((value, key) => {
          console.log(key, value);
        });
        const productRow = document.getElementById('product_' + this.editingProductId);
        if (productRow) {
          productRow.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        this.NavigationService.editProduct(this.editingProductId, formData).subscribe(
          response => {
            console.log('Product edited successfully!', response);
            this.productForm.reset();
            this.editingProductId = null;
            this.selectedFile = undefined;
            this.showEditSection = false;

          },
          error => {
            // console.error('Error editing product:', error);
            this.productForm.reset();
            this.editingProductId = null;
            this.selectedFile = undefined;
            this.showEditSection = false;
            this.fetchProducts();
          }
        );
      } else {
        console.error('No product is being edited.');
      }

  }


  public HandleFile(files: any): void {
    if (files?.length > 0) {
      const updatedFile = <File>files[0];
      this.productForm.controls["file"].setValue(updatedFile);
    } else {
      this.productForm.controls["file"].setValue("");
    }
  }



  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    console.log('Selected file:', this.selectedFile);
  }
}
