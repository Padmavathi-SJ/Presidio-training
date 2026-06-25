import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YieldReports } from './yield-reports';

describe('YieldReports', () => {
  let component: YieldReports;
  let fixture: ComponentFixture<YieldReports>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YieldReports],
    }).compileComponents();

    fixture = TestBed.createComponent(YieldReports);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
