import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GoogleLoginProvider, AuthService } from 'angularx-social-login';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient, private authService: AuthService) { }

  private baseUrlLogin = 'https://localhost:5001/account/login';
  private baseUrlLogout = 'https://localhost:5001/account/logout';

  public async Login(): Promise<User> {
    const googleResponse = await this.authService.signIn(GoogleLoginProvider.PROVIDER_ID);

    console.log(googleResponse);

    const response = await this.http.post<LoginResponse>(this.baseUrlLogin, googleResponse).toPromise();

    this.SetToken(response.authToken);
    this.SetUser(response.user);

    return Promise.resolve(response.user);
  }

  public async Logout(): Promise<void> {
    await this.authService.signOut();

    const response = await this.http.post(this.baseUrlLogout, null).toPromise();

    this.RemoveToken();
    this.RemoveUser();
    console.log('User has signed out');
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

class User {
  id: number;
  name: string;
  email: string;
  pictureUrl: string;
}

class LoginResponse {
  authToken: string;
  user: User;
}
