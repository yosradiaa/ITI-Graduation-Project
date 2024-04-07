import { User } from './../models/models';
import { Component, OnInit } from '@angular/core';
import { UtilityService } from '../services/utility.service';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  selectedUser: User | null = null;
  addUserForm: FormGroup;
  editUserForm: FormGroup; // Add FormGroup for edit form
  editingUser: User | null = null;
  showAddUserForm: boolean = false;
  showEditUserForm: boolean = false; // Flag to show/hide edit form
  userFormSubmitted: boolean = false;
  newUser: User = {
    id: 0,
    firstName: '',
    lastName: '',
    email: '',
    address: '',
    mobile: '',
    password: '',
    createdAt: '',
    modifiedAt: ''
  };

  constructor(private formBuilder: FormBuilder, private userService: UtilityService) {
    this.addUserForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      address: ['', Validators.required],
      mobile: [
        '',
        [Validators.required, Validators.pattern(/^(012|011|015)[0-9]{8}$/)],
      ],
      password: ['', Validators.required],
    });

    this.editUserForm = this.formBuilder.group({
      id: [''],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      address: ['', Validators.required],
      mobile: [
        '',
        [Validators.required, Validators.pattern(/^(012|011|015)[0-9]{8}$/)],
      ],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.getAllUsers();
  }

  getAllUsers(): void {
    this.userService.getAllUsers().subscribe(users => {
      this.users = users;
    });
  }

  deleteUser(userId: number): void {
    const index = this.users.findIndex(user => user.id === userId);
    if (index !== -1) {
      if (confirm("Are you sure you want to delete this user?")) {
        this.users.splice(index, 1);
        this.userService.deleteUser(userId).subscribe(response => {
          if (!response.success) {
            this.getAllUsers();
            console.error("Failed to delete user.");
          }
        });
      }
    }
  }

  editUser(user: User): void {
    this.selectedUser = user;
    this.editingUser = { ...user }; // Create a copy for editing
    this.showEditUserForm = true; // Show the edit form
    // Populate the edit form with selected user data
    this.editUserForm.patchValue({
      id: user.id,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      address: user.address,
      mobile: user.mobile,
      password: user.password,
    });
  }

  cancelEdit(): void {
    this.selectedUser = null;
    this.editingUser = null;
    this.showEditUserForm = false; // Hide the edit form
    this.editUserForm.reset(); // Reset the edit form
  }

  cancelAddUser(): void {
    this.showAddUserForm = false; // Hide the add user form
    this.resetAddUserForm(); // Reset the add user form
  }

  saveUserChanges(): void {
    this.userFormSubmitted = true;
    if (this.editUserForm.valid) {
      const updatedUser: User = {
        id: this.editUserForm.value.id,
        firstName: this.editUserForm.value.firstName,
        lastName: this.editUserForm.value.lastName,
        email: this.editUserForm.value.email,
        address: this.editUserForm.value.address,
        mobile: this.editUserForm.value.mobile,
        password: this.editUserForm.value.password,
        createdAt: this.editingUser?.createdAt ?? '', // Use optional chaining and nullish coalescing operator
        modifiedAt: new Date().toISOString() // Update modifiedAt timestamp to current time
      };
      // Update the user locally
      const index = this.users.findIndex(user => user.id === updatedUser.id);
      if (index !== -1) {
        this.users[index] = { ...updatedUser };
      }
      // Send request to update user on the server
      this.userService.editUser(updatedUser.id, updatedUser).subscribe(response => {
        if (response) {
          // If response is successful, no further action needed
          this.selectedUser = null;
          this.editingUser = null;
          this.showEditUserForm = false;
          this.editUserForm.reset();
          console.log("User updated successfully.");
          this.getAllUsers();
        } else {
          // If response is unsuccessful, revert the change in the local list
          this.users[index] = { ...this.selectedUser! };
          console.error("Failed to save user changes.");
          this.getAllUsers();
        }
      });
    }
  }

  // Other methods for add user form, validation, etc.
  isValidForm(): boolean {
    // Check if any input field is empty or if the mobile number doesn't meet the required pattern
    return !(
      !this.editingUser?.firstName ||
      !this.editingUser?.lastName ||
      !this.editingUser?.email ||
      !this.editingUser?.address ||
      !this.editingUser?.mobile ||
      !this.isValidMobile(this.editingUser?.mobile) // Check mobile number pattern
    );
  }

  isValidMobile(mobile: string | undefined): boolean {
    // Check mobile number pattern
    const mobilePattern = /^(012|011|015)[0-9]{8}$/;
    return !!mobile && mobilePattern.test(mobile);
  }

  toggleAddUserForm(): void {
    this.showAddUserForm = !this.showAddUserForm;
    // Reset newUser object when hiding the form
    if (!this.showAddUserForm) {
      this.resetNewUser();
    }
  }

  resetNewUser(): void {
    // Reset newUser object to clear form inputs
    this.newUser = {
      id: 0,
      firstName: '',
      lastName: '',
      email: '',
      address: '',
      mobile: '',
      password: '',
      createdAt: '',
      modifiedAt: ''
    };
  }

  resetAddUserForm(): void {
    this.addUserForm.reset();
  }

  addUser(): void {
    const newUser: User = {
      id: 0, // Set the ID to 0 for now, it will be updated after adding to the database
      firstName: this.FirstName.value,
      lastName: this.LastName.value,
      email: this.Email.value,
      address: this.Address.value,
      mobile: this.Mobile.value,
      password: this.Password.value,
      createdAt: new Date().toISOString(), // Set createdAt timestamp to current time
      modifiedAt: new Date().toISOString() // Set modifiedAt timestamp to current time
    };

    if (this.addUserForm.valid) {
      this.userService.addUser(newUser).subscribe(
        (response: any) => {
          console.log('User added successfully:', response);
          if (response && response.text === "User inserted successfully") {
            // Success message received
            this.refreshPage();
            this.users.push(newUser); // Manually add the new user to the users array
            this.resetAddUserForm(); // Reset the form
            this.getAllUsers();
          } else {
            console.error('Error adding user: Unexpected server response');
          }
        },
        (error: any) => {
          console.error('Error adding user:', error);
          this.getAllUsers();
        }
      );
    } else {
      this.addUserForm.markAllAsTouched();
    }
  }



  refreshPage(): void {
    window.location.reload();
  }

  get FirstName(): FormControl {
    return this.addUserForm.get('firstName') as FormControl;
  }
  get LastName(): FormControl {
    return this.addUserForm.get('lastName') as FormControl;
  }
  get Email(): FormControl {
    return this.addUserForm.get('email') as FormControl;
  }
  get Address(): FormControl {
    return this.addUserForm.get('address') as FormControl;
  }
  get Mobile(): FormControl {
    return this.addUserForm.get('mobile') as FormControl;
  }
  get Password(): FormControl {
    return this.addUserForm.get('password') as FormControl;
  }
}
