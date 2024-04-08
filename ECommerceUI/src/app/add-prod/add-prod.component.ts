import { Component, OnInit } from '@angular/core';
import { NavigationService } from './../services/navigation.service';

  import { HttpClient } from '@angular/common/http';
  import { Product, Category, Offer } from '../models/models';
  import { HttpHeaders } from '@angular/common/http';
  import { Router } from '@angular/router';
  import { FormControl, FormGroup, Validators } from '@angular/forms';
  import { response } from 'express';
  
@Component({
  selector: 'app-add-prod',
  templateUrl: './add-prod.component.html',
  styleUrls: ['./add-prod.component.css']
})
export class AddProdComponent implements OnInit {

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
  
    constructor(private http: HttpClient , private router: Router, private NavigationService: NavigationService) { }
  
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
  
    onSubmit(productForm: FormGroup): void {
      console.log(productForm);
      if(productForm.valid){
        const productFormValues = productForm.value;
        console.log(productFormValues);
        const formData = new FormData();
        for (const key in productFormValues) {
          formData.append(key , productFormValues[key]);
        }
        this.NavigationService.addProduct(formData).subscribe((response)=> {
          console.log(response);
          this.fetchProducts();
          this.resetForm();
        })
      }
    }
  
  
    onImageSelected(event: any): void {
      this.imageFile = event.target.files[0];
    }
  
    saveImageLocally(imagePath: string): void {
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
  
  
    public HandleFile(files:any){
      if(files?.length > 0){
  
        const updatedFile = <File>files[0];
        this.productForm.controls["file"].setValue(updatedFile);
      }
      else
      {
        this.productForm.controls["file"].setValue("");
      }
  
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
  
   
  
  
  
  
    
}
