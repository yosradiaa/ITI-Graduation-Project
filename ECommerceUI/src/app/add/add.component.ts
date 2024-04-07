import { Component, OnInit } from '@angular/core';
import { Offer, Product, Category } from '../models/models';
import { HttpClient } from '@angular/common/http';
import { Router } from 'express';
import { NavigationService } from './../services/navigation.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-add',
  templateUrl: './add.component.html',
  styleUrls: ['./add.component.css']
})
export class AddComponent implements OnInit {

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


  public productForm:FormGroup = new FormGroup({
    title:new FormControl("" , [Validators.required]),
    price:new FormControl("" , [Validators.required]),
    quantity:new FormControl("" , [Validators.required]),
    offer:new FormControl("" , [Validators.required]),
    productCategory:new FormControl("" , [Validators.required]),
    description:new FormControl("" , [Validators.required]),
    file:new FormControl("" , [Validators.required]),
  });



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

}
