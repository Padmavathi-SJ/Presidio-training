import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeBannerComponent } from './home-banner';

describe('HomeBannerComponent', () => {
  let component: HomeBannerComponent;
  let fixture: ComponentFixture<HomeBannerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeBannerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeBannerComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
