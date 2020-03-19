import { Injectable } from '@angular/core';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class AppInitService {

  constructor(private accountService: AccountService) { }

  public async Login(): Promise<void> {
    const token = this.accountService.GetToken();
    if (token) {
      await this.accountService.LoginByToken(token);
    }
  }
}
