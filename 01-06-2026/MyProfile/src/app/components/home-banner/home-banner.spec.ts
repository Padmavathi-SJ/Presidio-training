import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeBanner } from './home-banner.component';

describe('HomeBanner', () => {
  let component: HomeBanner;
  let fixture: ComponentFixture<HomeBanner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeBanner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeBanner);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
