import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Tag } from '../models/tag';
import { TagService } from '../services/tag.service';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'mg-tags-selector',
  templateUrl: './tags-selector.component.html',
  styleUrls: ['./tags-selector.component.css']
})
export class TagsSelectorComponent implements OnInit {

  currentTagName: string = "";

  relatedTags: Map<string, Tag> = new Map<string, Tag>();

  tags: Map<string, Tag> = new Map<string, Tag>();

  constructor(private tagService: TagService, private accountService: AccountService) { }

  async ngOnInit() {
    this.accountService.LoginStateChanged$.subscribe(async (logedin) => {
      if (logedin) {
        const tags = await this.tagService.getMyTags();

        for (let tag of tags) {
          this.addTag(tag);
        }
      } else {
        this.tags.clear();
        this.relatedTags.clear();
      }
    });
  }

  async addCurrentTag(): Promise<any> {
    if (!this.currentTagName) {
      return;
    }

    if (!this.tags.get(this.currentTagName)) {
      let newTag: Tag = { 'name': this.currentTagName, 'url': null, 'parentTagName': null };
      this.tags.set(this.currentTagName, newTag);
      await this.getRelatedTags(newTag);
    }

    this.currentTagName = "";
  }

  async addTag(tag: Tag): Promise<any> {
    if (!this.tags.get(tag.name)) {
      this.tags.set(tag.name, tag);
      await this.getRelatedTags(tag);
    }

    this.currentTagName = "";
  }

  async addRelatedTag(tag: Tag): Promise<any> {
    if (!this.relatedTags.get(tag.name)) {
      this.relatedTags.set(tag.name, tag);
    }
  }

  onRemoveTag(tag: Tag): void {
    this.tags.delete(tag.name);
    this.relatedTags.delete(tag.name);
  }

  onClickTag(tag: Tag): void {
    //
  }

  onRemoveRelatedTag(tag: Tag): void {
    this.relatedTags.delete(tag.name);
  }

  onClickRelatedTag(tag: Tag): void {
    this.addTag(tag);
    this.onRemoveRelatedTag(tag);
  }

  async save(): Promise<void> {
    const tagNames = Array.from(this.tags.values()).map(t => t.name);
    return this.tagService.setMyTags(tagNames);
  }

  private async getRelatedTags(tag: Tag): Promise<void> {
    let relatedTags = await this.tagService.getRelatedTags(tag.name);
    for (let relatedTag of relatedTags) {
      await this.addRelatedTag(relatedTag);
    }
  }

}
