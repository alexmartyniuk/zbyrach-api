import { Component, OnInit } from '@angular/core';
import { AuthService } from 'angularx-social-login';
import { GoogleLoginProvider } from 'angularx-social-login';
import { AccountService } from '../services/account.service';
import { User } from '../models/user';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  public IsLogedIn: boolean = false;
  public UserName: string = "";
  public UserPictureUrl: string = "";

  constructor(private accountService: AccountService) { }

  async ngOnInit() {
    const token = this.accountService.GetToken();
    if (token) {
      const user = await this.accountService.LoginByToken(token);
      this.setLogedIn(user);
      console.log('User has loged in by token');
    }
  }

  public async LogInWithGoogle(): Promise<void> {
    try {
      const user = await this.accountService.LoginWithGoogle();
      this.setLogedIn(user);

      console.log('User has loged in with Google');
    } catch (error) {
      console.log(error);
    }
  }

  public async LogOut(): Promise<void> {
    try {
      await this.accountService.Logout();
      this.setLogedOut();
      console.log('User has loged out');
    }
    catch (error) {
      console.log(error);
    }
  }

  private setLogedIn(user:User) {
    this.UserName = user.name;
    this.UserPictureUrl = user.pictureUrl;

    this.IsLogedIn = true;
  }

  private setLogedOut() {
    this.UserName = "";
    this.UserPictureUrl = "";

    this.IsLogedIn = false;
  }
}
