import { Injectable } from '@angular/core';
import { Tag } from '../models/tag';
import { HttpClient } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class TagService {

  constructor(private api: ApiService) { }

  public async getRelatedTags(name: string): Promise<Tag[]> {
    let relatedTags: Tag[] = await this.api.GetRelatedTags(name);

    for (let relatedTag of relatedTags) {
      relatedTag.parentTagName = name;
    }

    return relatedTags;
  }

  public async getMyTags(): Promise<Tag[]> {
    return this.api.GetMyTags();
  }

  public async setMyTags(tags: string[]): Promise<void> {
    return this.api.SetMyTags(tags);
  }
}
