import { Injectable } from '@angular/core';
import { GoogleLoginProvider, AuthService } from 'angularx-social-login';
import { ApiService } from './api.service';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private api: ApiService, private authService: AuthService) { }

  public async LoginWithGoogle(): Promise<User> {
    try {
      const googleResponse = await this.authService
        .signIn(GoogleLoginProvider.PROVIDER_ID);
      return this.LoginByToken(googleResponse.idToken);
    } catch (e) {
      return Promise.reject(e);
    }
  }

  public async LoginByToken(token: string): Promise<User> {
    try {
      const response = await this.api.Login(token);

      this.SetToken(response.token);
      this.SetUser(response.user);

      return Promise.resolve(response.user);
    } catch (e) {
      this.RemoveToken();
      this.RemoveUser();
      return Promise.reject(e);
    }
  }

  public async Logout(): Promise<any> {
    try {
      await this.authService.signOut();
      await this.api.Logout();

      this.RemoveToken();
      this.RemoveUser();
      return Promise.resolve();
    } catch (e) {
      return Promise.reject(e);
    }
  }

  public GetToken(): string {
    return localStorage.getItem('token');
  }

  private SetToken(token: string) {
    localStorage.setItem('token', token);
  }

  private RemoveToken() {
    localStorage.removeItem('token');
  }

  public GetUser(): User {
    return JSON.parse(localStorage.getItem('user'));
  }

  private SetUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
  }

  private RemoveUser() {
    localStorage.removeItem('user');
  }
}
