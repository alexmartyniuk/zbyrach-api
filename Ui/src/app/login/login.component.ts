import { Component, OnInit } from '@angular/core';
import { AuthService } from 'angularx-social-login';
import { GoogleLoginProvider } from 'angularx-social-login';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  //create array to store user data we need
  userData: any[] = [];

  // create a field to hold error messages so we can bind it to our        template
  resultMessage: string;

  constructor(private authService: AuthService, private accountService: AccountService) { }

  ngOnInit() {
    //some code
  }

  //logIn with google method. Takes the platform (Google) parameter.
  public LogInWithGoogle(): void {
    this.accountService.Login().then(
      (user) => {
        console.log('success', user);
        this.resultMessage = user.email;
      },
      (error) => {
        this.resultMessage = error;
        console.log(error);
      }
    );
  }

  public LogOut(): void {
    this.accountService.Logout().then(
      () => {
        console.log('User has signed out');
      },
      (error) => {
        this.resultMessage = error;
        console.log(error);
      }
    );
  }
}
