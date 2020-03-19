import { Component, OnInit } from '@angular/core';
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
    this.accountService.LoginStateChanged$.subscribe((logedin) => {
      if (logedin) {
        const user = this.accountService.GetUser();
        this.setLogedIn(user);
      } else {
        this.setLogedOut();
      }
    });
  }

  public async LogInWithGoogle(): Promise<void> {
    try {
      await this.accountService.LoginWithGoogle();
    } catch (error) {
      console.log(error);
    }
  }

  public async LogOut(): Promise<void> {
    try {
      await this.accountService.Logout();
    }
    catch (error) {
      console.log(error);
    }
  }

  private setLogedIn(user: User) {
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
